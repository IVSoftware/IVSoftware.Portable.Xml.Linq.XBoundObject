[◀](../README.md)


## Placer Class Overview

The `Placer` class is designed to project a flat string path onto a new or existing node within an XML hierarchy, effectively managing intermediate paths and their creation as necessary. Using this class directly provides access to options and callbacks, but in many cases (like the file viewer below) there are high-level extensions that make using the `Placer` class more convenient and accessible.

The MSTest example in the snippet below is part of this repo, which can be cloned and run to facilitate "hands-on" learning.

```
/// <summary>
/// Place nodes using Placer.DefaultNewXElementName.
/// </summary>    
[TestMethod]
// <PackageReference Include="IVSoftware.Portable.Xml.Linq.XBoundObject" Version="2.0.*" />
public void Test_RawPlacerClassUsage()
{
    string actual, expected;

    string winPath = @"C:\Users\Public\Documents";
    XElement xroot = new XElement("root");

    // Options explained
    var placer = new Placer(
        xSource: xroot,
        fqpath: winPath,
        // optional
        onBeforeAdd: (sender, e) =>
        {
            // Modify, substitute, or cancel new `XElement`
            // nodes before they are added.
            if(e.IsPathMatch)
            {
                // The final destination node.
            }
            else 
            { 
                // Ad hoc nodes added to achieve the full path.
            }
        },            
        // optional
        onAfterAdd: (sender, e) =>
        {
            // Work with 
        },
        // optional
        onIterate: (sender, e) =>
        {
            // Work with the traversal regardless of whether
            // new nodes are being created.
        },
        // The default mode is FindOrCreate.
        mode: PlacerMode.FindOrCreate,
        // Dictates which XAttribute value to use for constructing paths.
        pathAttributeName: "text");


    switch (placer.PlacerResult)
    {
        case PlacerResult.Created:
            break;
        case PlacerResult.NotFound:
        case PlacerResult.Partial:
        case PlacerResult.Exists:
        case PlacerResult.Assert:
        case PlacerResult.Throw:
            Assert.Fail("Expecting Created");
            break;
    }
    actual = xroot.ToString();
    actual.ToClipboardAssert("Expecting path is 2D tree now.");
    { }
    expected = @" 
<root>
  <xnode text=""C:"">
    <xnode text=""Users"">
      <xnode text=""Public"">
        <xnode text=""Documents"" />
      </xnode>
    </xnode>
  </xnode>
</root>";
    // Uses IVSoftware.WinOS.MSTest.Extensions.NormalizeResult() for whitespace tolerance.
    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting path is 2D tree now."
    );
}
```

### Introduction to Placer Extensions

This intro will focus on four extensions. The first two apply to any `T`.
1. `FindOrCreate()` - Ensures that an element of `DefaultNewXElementName` exists at the specified path.
2. `FindOrCreate<T>()` - Binds an instance of `T` to the destination node, and to any passing node that doesn't already have a `T`.

The second two apply when `T is IXBoundViewObject`:
3. `Show()` - Makes the target element visible by running `Placer` in `FindOrThrow` mode (i.e. `InvalidOperationException` if not found).
4. `Show<T>()` - Follows the rules of `FindOrCreate<T>()` then sets `IXBoundViewObject.IsVisible` to true.

Consider something like a File System Viewer which would be a good example of hierarchal data that may need to be displayed not only in MAUI but also in frameworks like WinForms or WPF. There will also be a need to manipulate this data (e.g. Drag Drop) while keeping it decoupled from the UI. This can be facilitated using an inherently recursive runtime data structure like `System.Xml.Linq.XElement` that is a universal in all of .NET and so brings portability to the solution.

For something like a filesystem, the view doesn't need to recurse just because the data does. We're just looking for an accurate representation without actually coupling the data to the view. A runtime `System.Xml.Linq.XElement` is a good fit to hold the actual data. This snippet is _portable_ code. It actually runs in an MSTest project. It demonstrates the use of helper extensions from [XBoundObject](https://www.nuget.org/packages/IVSoftware.Portable.Xml.Linq.XBoundObject/2.0.0) to achieve three basic objectives:

1. Converts flat file paths to a 2D representation on a standard `System.Xml.Linq.XElement`.
2. Uses `ViewContext.SyncList()` to synchronize an observable collection (this will be the `CollectionView` source) to the visible items in the `XElement` hierarchy.
3. For testing purposes, uses the `ViewContext.ToString()` to "print out" the observable collection.

This code also exercises the `PlusMinusToggleCommand`.

So lets start with the portable (Maui, WinForms, WPF) view of things, then below it I'll show the non-recursive `DataTemplate` that would be plug-and-play with the `FSItems` observable collection shown.

___

```
class DriveItem : XBoundViewObjectImplementer { }
class FolderItem : XBoundViewObjectImplementer { }
class FileItem : XBoundViewObjectImplementer { }

/// <summary>
/// Basic File System manipulations.
/// </summary>
[TestMethod]
public void Test_BasicUsageExamples_101()
{
    string actual, expected;
    FolderItem currentFolder = null;

    // Filesystem items
    var FSItems = new ObservableCollection<XBoundViewObjectImplementer>();
    var XRoot = new XElement("root");
    var ViewContext = new ViewContext(XRoot, FSItems, indent: 2, autoSyncEnabled: false);

    // Bind the ViewContext to the root element.
    XRoot.SetBoundAttributeValue(ViewContext);

    actual = XRoot.ToString();
    expected = @" 
<root viewcontext=""[ViewContext]"" />";

    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting ViewContext instance is bound to Root."
    );
    Assert.IsInstanceOfType<ViewContext>(
        XRoot.To<ViewContext>(),
        "Expecting loopback of the ViewContext instance.");

    // Get Environment.SpecialFolder locations on C: drive only
    var specialFolderPaths =
        Enum
        .GetValues<Environment.SpecialFolder>()
        .Select(_ => Environment.GetFolderPath(_))
        .Where(_ => _.StartsWith("C", StringComparison.OrdinalIgnoreCase))
        .ToList();
    { }

    // Set a DriveItem at the root level.
    XRoot.Show<DriveItem>("C:");

    // Place the folder paths in the XML hierarchy.
    specialFolderPaths
        .ForEach(_ => XRoot.FindOrCreate<FolderItem>(_));

    // Now that the filesystem is populated, update the +/-
    // based on the nested (but not visible) folder items.
    DriveItem cDrive = XRoot.FindOrCreate<DriveItem>("C:");

        
    Assert.AreEqual(
        PlusMinus.Collapsed,
        cDrive.Expand(ExpandDirection.FromItems),
        "Expecting found folders result in Collapsed (not Leaf) state.");

    // Manually synchronize the observable collection.
    // This is necessary because we initialized AutoSyncEnabled to false.
    ViewContext.SyncList();

    // View the observable collection, synchronized to Visible Items.
    actual = ViewContext.ToString();
    actual.ToClipboardExpected();
    expected = @" 
+ C:"
    ;
    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting collapsed C: drive"
    );

    // Perform a click on the C drive to expand the node.
    cDrive.PlusMinusToggleCommand?.Execute(cDrive);

    // View the observable collection, synchronized to Visible Items.
    ViewContext.SyncList();
    actual = ViewContext.ToString();
    expected = @" 
- C:
+ Program Files
+ Program Files (x86)
+ ProgramData
+ Users
+ WINDOWS"
    ;

    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting C: is now expanded at root."
    );

    // Navigate to Program Files and "click" on it to expand.
    currentFolder = XRoot
        .FindOrCreate<FolderItem>(Path.Combine("C:", "Program Files"));
    currentFolder?.PlusMinusToggleCommand.Execute(currentFolder);

    // View the observable collection, synchronized to Visible Items.
    ViewContext.SyncList();
    actual = ViewContext.ToString();

    expected = @" 
- C:
- Program Files
    Common Files
+ Program Files (x86)
+ ProgramData
+ Users
+ WINDOWS"
    ;

    Assert.AreEqual(
        expected.NormalizeResult(),
        actual.NormalizeResult(),
        "Expecting expanded Program Files to have a child folder that is a Leaf i.e. empty."
    );
}
```

**Non-Recursive DataTemplate for Maui.Windows and Maui.Android**


These screenshots show how a non-recursive data template that indents the text label based on the current depth in the XML tree create the "smoke-and-mirrors" illusion of depth. To review, we started with flat file path strings, we are maintaining that data in a functional `XElement` tree hierarchy that is _not_ visible, but we iterate the visible items of `XRoot.Descendants()` to maintain an observable collection that _is_ flat but _appears to be_ 2D.


[ ]    [ ]

___

Here's the shared xaml for these views.

```
<CollectionView 
    x:Name="FileCollectionView" 
    ItemsSource="{Binding FSItems}" 
    SelectionMode="None"
    SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
    BackgroundColor="AliceBlue"
    Margin="1">
    <CollectionView.ItemsLayout>
        <LinearItemsLayout Orientation="Vertical" ItemSpacing="2" />
    </CollectionView.ItemsLayout>
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid ColumnDefinitions="Auto,40,*" RowDefinitions="40" >
                <BoxView 
                    Grid.Column="0" 
                    WidthRequest="{Binding Space}"
                    BackgroundColor="{
                        Binding BackgroundColor, 
                        Source={x:Reference FileCollectionView}}"/>
                <Button 
                    Grid.Column="1" 
                    Text="{Binding PlusMinusGlyph}" 
                    FontSize="16"
                    FontFamily="file-folder-drive-icons"
                    BackgroundColor="Transparent"
                    Padding="0"
                    VerticalOptions="Fill"
                    HorizontalOptions="Fill"
                    MinimumHeightRequest="0"
                    MinimumWidthRequest="0"
                    CornerRadius="0"
                    Command="{Binding PlusMinusToggleCommand}"
                    CommandParameter="{Binding .}">
                    <Button.TextColor>
                        <MultiBinding Converter="{StaticResource ColorConversions}">
                            <Binding />
                            <Binding Path="PlusMinus"/>
                        </MultiBinding>
                    </Button.TextColor>
                </Button>
                <Label 
                    Grid.Column="2"
                    Text="{Binding Text}" 
                    VerticalTextAlignment="Center" Padding="2,0,0,0"/>
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```
