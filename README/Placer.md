[◀](../README.md)


## Placer Class Overview

The `Placer` class is designed to project a flat string path onto a new or existing node within an XML hierarchy, effectively managing intermediate paths and their creation as necessary.

### Basic Usage

Files and Folders are a good example of hierarchal data that may need to be displayed not only in MAUI but also in frameworks like WinForms or WPF. There will also be a need to manipulate this data (e.g. Drag Drop) while keeping it decoupled from the UI. This can be facilitated using an inherently recursive runtime data structure like `System.Xml.Linq.XElement` that is a universal in all of .NET and so brings portability to the solution.

Let's jump right in by declaring our "tree".

`XElement XRoot {get;} = new ("root")`;
___

### Recursion in a Tree Model not in the View

The `XElement` instance comes with a built-in method to traverse the entire tree (which won't always be empty):
```
foreach(XElement xel in XRoot.Descendants())
{
    // Interact with the XElement
}
```
___

#### Projecting a Flat Path to Two Dimensions

The first requirement is to take a "flat" representation (i.e. the file path) and efficiently place it relative to the root `XElement`. One of many ways to do this is to use the NuGet package for [XBoundObject](https://www.nuget.org/packages/IVSoftware.Portable.Xml.Linq.XBoundObject/1.4.1-rc). Here we'll take the project path and "project" it to 2D using the `Place` extension.

```
// <PackageReference 
//     Include = "IVSoftware.Portable.Xml.Linq.XBoundObject" 
//     Version="1.4.1-rc" />
XElement xroot = new XElement("root"); 
string path =
    @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj"
    .Replace('\\', Path.DirectorySeparatorChar); // Real code uses Path.Combine()
xroot.Place(path);
var expected = xroot.ToString();
```

Inspecting the value of `expected` now shows the path as a tree.

```xml
<root>
  <xnode text="C:">
    <xnode text="files-and-folders">
      <xnode text="FilesAndFolders">
        <xnode text="FilesAndFolders.csproj" />
      </xnode>
    </xnode>
  </xnode>
</root>
```
___

#### From Tree Model to View

Now just use standard `System.Xml.Linq` to traverse this, adding a `FileItem` to `FileItems` for each node. For example, a data template for MAUI `CollectionView` can simply provide a spacer whose _width_ is bound to the _depth_ of the node, creating the smoke and mirrors illusion of a tree.

```
foreach (var xel in BindingContext.XRoot.Descendants())
{
    FileItems.Add(new FileItem
    {
        Text = xel.Attribute("text")?.Value ?? "Error",
        PlusMinus = 
        ReferenceEquals(xel, newXel)
            ? string.Empty
            : "-",
        Space = 10 * xel.Ancestors().Skip(1).Count(),
    });
}
```

[![android screenshot](https://stackoverflowteams.com/c/sqdev/images/s/6ac025ff-00ed-4e00-b19f-690014b6c83d.png)](https://stackoverflowteams.com/c/sqdev/images/s/6ac025ff-00ed-4e00-b19f-690014b6c83d.png)
___

#### Data Template

There is no need for recursion here. The width of the `BoxView` creates the 2-dimensional effect.

```
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
        Text="{Binding PlusMinus}" 
        TextColor="Black"
        Command="{
            Binding PlusMinusToggleCommand, 
            Source={x:Reference MainPageViewModel}}"
        CommandParameter="{Binding .}"
        FontSize="16"
        BackgroundColor="Transparent"
        Padding="0"
        BorderWidth="0"
        VerticalOptions="Fill"
        HorizontalOptions="Fill"
        MinimumHeightRequest="0"
        MinimumWidthRequest="0"
        CornerRadius="0"/>
        <Label 
        Grid.Column="2"
        Text="{Binding Text}" 
        VerticalTextAlignment="Center" Padding="2,0,0,0"/>
    </Grid>
</DataTemplate>
```
#### Data Model

The `Space` property controls the indentation shown on the view.

```
class FileItem : INotifyPropertyChanged
{
    public string Text
    {
        get => _text;
        set
        {
            if (!Equals(_text, value))
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }
    string _text = string.Empty;

    public string PlusMinus
    {
        get => _plusMinus;
        set
        {
            if (!Equals(_plusMinus, value))
            {
                _plusMinus = value;
                OnPropertyChanged();
            }
        }
    }
    string _plusMinus = "+";

    public int Space
    {
        get => _space;
        set
        {
            if (!Equals(_space, value))
            {
                _space = value;
                OnPropertyChanged();
            }
        }
    }
    int _space = default;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public event PropertyChangedEventHandler? PropertyChanged;
}
```
___

## Manipulating the Tree Model (PlusMinus)

When the time comes to do something meaningful, like collapsing the node, the `PlusMinusToggleCommand` will require access to the corresponding `XElement`. So first, add an `XEL` property to `FileItem` that gets initialized in its CTOR and then bind the instance of `FileItem` to XEL using `XBoundObject`.

```
class FileItem : INotifyPropertyChanged
{
    public FileItem(XElement xel)
    {
        XEL = xel;
        xel.SetBoundAttributeValue(this);
    }
    public XElement XEL { get; }
    .
    .
    .
}
```

Then, implement the command in the `MainPageViewModel`. If the current value is `-` it indicates that the node is currently expanded and that descendant `FileItem` models in the `FileItems` collection should be removed. If the value is `+` then descendant items need to be recursively added back in where an item's visibility is true if its parent item's `PlusMinus` is `-`;

```
public ICommand PlusMinusToggleCommand
{
    get
    {
        if (_plusMinusToggleCommand is null)
        {
            _plusMinusToggleCommand = new Command<FileItem>((fileItem) =>
            {
                switch (fileItem.PlusMinus)
                {
                    case "+":
                        var index = FileItems.IndexOf(fileItem);
                        foreach (
                            var child in 
                            fileItem.XEL.Elements()
                            .Where(_=>_.Has<FileItem>()))
                        {
                            localRecursiveAdd(child);

                            #region L o c a l F x       
                            void localRecursiveAdd(XElement current)
                            {
                                index++;
                                var add = current.To<FileItem>();
                                FileItems.Insert(index, add);
                                if(add.PlusMinus == "-")
                                {
                                    foreach (
                                        var nested in
                                        add.XEL.Elements()
                                        .Where(_ => _.Has<FileItem>()))
                                    {
                                        localRecursiveAdd(nested);
                                    }
                                }
                            }		
                            #endregion L o c a l F x
                        }
                        fileItem.PlusMinus = "-";
                        break;
                    case "-":
                        foreach (var desc in fileItem.XEL.Descendants())
                        {
                            if (desc.To<FileItem>() is { } remove)
                            {
                                FileItems.Remove(remove);
                            }
                        }
                        fileItem.PlusMinus = "+";
                        break;
                    default:
                        Debug.Fail("Unexpected");
                        break;
                }
            });
        }
        return _plusMinusToggleCommand;
    }
}
ICommand? _plusMinusToggleCommand = null;
```

___

## In-Depth Example Code

The [FilesAndFolders]() repo contains a functional file browser example that uses the same portable backend tree for Maui (tested for Android and Windows) and for WinForms.


