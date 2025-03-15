using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using static IVSoftware.Portable.Threading.Extensions;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections.Specialized;
using System.Xml.Linq;
using IVSoftware.Portable.Threading;
using System.Collections;
using System.Collections.ObjectModel;
using XBoundObjectMSTest.TestClassesForModeling.Common;
using System.ComponentModel;
using System.Diagnostics;
using XBoundObjectMSTest.TestClassesForModeling.Test_Singleton;
using XBoundObjectMSTest.TestClassesForModeling.Test_ModelInit;
using IVSoftware.Portable.Xml.Linq;

namespace XBoundObjectMSTest;
internal enum CallerName
{
    WithNotifyOnDescendants,
}

[TestClass]
public class TestClass_Modeling
{
    #region S E T U P
    [Flags]
    internal enum ClearQueue
    {
        PropertyChangedEvents = 0x1,
        NotifyCollectionChangedEvents = 0x2,
        XObjectChangeEvents = 0x4,
        OnAwaitedEvents = 0x8,
        All = 0xF
    }
    CallerName[] BlockedCallers { get; set; } = [];
    string actual = string.Empty, expected = string.Empty, joined = string.Empty;
    XElement? model = null;
    SenderEventPair currentEvent = null!;
    Queue<SenderEventPair> eventsPC = new();
    Queue<SenderEventPair> eventsCC = new();
    Queue<SenderEventPair> eventsXO = new();
    Queue<SenderEventPair> eventsOA = new();
    void clearQueues(ClearQueue clearQueue = ClearQueue.PropertyChangedEvents | ClearQueue.NotifyCollectionChangedEvents)
    {
        if (clearQueue.HasFlag(ClearQueue.PropertyChangedEvents)) eventsPC.Clear();
        if (clearQueue.HasFlag(ClearQueue.NotifyCollectionChangedEvents)) eventsCC.Clear();
        if (clearQueue.HasFlag(ClearQueue.XObjectChangeEvents)) eventsXO.Clear();
        if (clearQueue.HasFlag(ClearQueue.OnAwaitedEvents)) eventsOA.Clear();
    }
    Random rando = new Random(1);
    private void OnAwaited(object? sender, AwaitedEventArgs e)
    {
        if(Enum.TryParse(e.Caller, out CallerName caller))
        {
            if(BlockedCallers.Contains(caller))
            {
                return;
            }
        }
        eventsOA.Enqueue(new SenderEventPair(sender ?? throw new NullReferenceException(), e));
    }
    [TestInitialize]
    public void TestInitialize()
    {
        Awaited += OnAwaited;
        actual = expected = joined = string.Empty;
        rando = new Random(1);
        clearQueues();
        BlockedCallers = [];
    }

    [TestCleanup]
    public void TestCleanup() => Awaited -= OnAwaited;
    #endregion S E T U P

    /// <summary>
    /// Unit test for verifying the event-driven XML model representation of `ClassA`.
    /// This test ensures that:
    /// 1. Adding instances of `ClassB` to `BCollection` triggers collection change events.
    /// 2. Changing the `Cost` property of `ClassC` within `BCollection` triggers property change events.
    /// 3. The XML model correctly reflects the structure and event subscriptions.
    /// 4. The calculated `C.Cost` totals match expected values.
    /// </summary>
    [TestMethod]
    public void Test_SO_79467031_5438626()
    {
        // Make an instance of class A which calls "The Extension" in its CTor.
        ClassA A = new(true);

        // Queue received PropertyChanged events so we can test them
        A.PropertyChanged += (sender, e) => eventsPC.Enqueue(
            new SenderEventPair(sender ?? throw new NullReferenceException(), e));

        // Queue received NotifyCollectionChanged events so we can test them
        A.CollectionChanged += (sender, e) => eventsCC.Enqueue(
            new SenderEventPair(sender ?? throw new NullReferenceException(), e));

        // EXPECT
        // - ClassA is at the root.
        // - ClassA is shown to have a TotalCost property.
        // - The `notify info` attribute contains the delegates to invoke when child elements raise events.
        // - There is an empty BCollection below it.
        // - BCollection has been identified as a source of INotifyPropertyChanged and INotifyCollectionChanged events.
        // - BCollection is also shown to have a `Count` property that is `Int32`.
        subtestInspectInitialModelForClassA();
        // EXPECT
        // - AdHoc ClassA is at the root.
        // Output is as before, but with full names for types.
        subtestInspectInitialModelForClassAWithFullTypeNames();
        // EXPECT
        // - An instance of ClassB is added to BCollection.
        // - A NotifyCollectionChanged event is triggered with action `Add`.
        // - A PropertyChanged event is triggered for the `Count` property of BCollection.
        // - The new ClassB instance appears in the model within BCollection.
        // - ClassB contains an instance of ClassC with observable properties Cost and Currency.
        subtestAddClassBThenViewModel();

        BlockedCallers = [CallerName.WithNotifyOnDescendants];
        clearQueues(ClearQueue.OnAwaitedEvents);

        // EXPECT
        // - The Cost property in ClassC is updated.
        // - A PropertyChanged event is triggered for the Cost property.
        // - The TotalCost property of ClassA is recalculated accordingly.
        subtestExerciseNestedCostProperty();
        // EXPECT
        // - A new instance of ClassB is created and assigned an initial Cost value.
        // - The instance is added to BCollection.
        // - The TotalCost property of ClassA is updated to reflect the new cost.
        subtestAddClassBInstanceWithNonZeroInitialCost();
        // EXPECT
        // - Three new instances of ClassB are created and added to BCollection.
        // - A total of five ClassB instances should now be visible in the model.
        // - Three NotifyCollectionChanged events with action `Add` are triggered.
        // - Three PropertyChanged events for `Count` are triggered as BCollection updates.
        // - Each added ClassB contains a ClassC instance with observable properties Cost and Currency.
        // - Updating the Cost property in each new ClassC instance triggers five cost updates.
        // - The cumulative TotalCost value should be updated accordingly.
        subtestAdd3xClassBInstanceWithZeroInitialCost();
        // EXPECT
        // - Iterates through all descendants of A.OriginModel that contain ClassC.
        // - Computes the total cost by summing up the Cost property of each ClassC instance.
        // - Validates that the expected total cost is 73905.
        subtestTryIteratingForClassC();

        BlockedCallers = [];
        clearQueues(ClearQueue.OnAwaitedEvents);

        // EXPECT
        // - The last item in BCollection is retrieved and removed.
        // - A NotifyCollectionChanged event with action `Remove` is triggered.
        // - Unsubscribe events are processed for the removed item and its dependencies.
        // - Further property changes on the removed instance do not trigger PropertyChanged events.
        subtestRemoveLastItemAndVerifyUnsubscribe();
        // EXPECT
        // - All items in BCollection are retrieved and then removed using Clear().
        // - The model should reflect the empty state of BCollection.
        // - Multiple unsubscribe events should be triggered for each removed instance.
        // - Further property changes on the removed instances do not trigger PropertyChanged events.
        subtestClearListAndVerifyUnsubscribe();

        #region S U B T E S T S
        // EXPECT
        // - ClassA is at the root.
        // - ClassA is shown to have a TotalCost property.
        // - The `notify info` attribute contains the delegates to invoke when child elements raise events.
        // - There is an empty BCollection below it.
        // - BCollection has been identified as a source of INotifyPropertyChanged and INotifyCollectionChanged events.
        // - BCollection is also shown to have a `Count` property that is `Int32`.
        void subtestInspectInitialModelForClassA()
        {
            actual = A.OriginModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" pi=""[Int32]"" />
  <member name=""BCollection"" pi=""[ObservableCollection]"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"">
    <member name=""Count"" pi=""[Int32]"" />
  </member>
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model of ClassA with TotalCost property and the BCollection which is empty"
            );
        }

        // EXPECT
        // - AdHoc ClassA is at the root.
        // Output is as before, but with full names for types.
        void subtestInspectInitialModelForClassAWithFullTypeNames()
        {
            ClassA classAwithFullNames = new(false);
            _ = classAwithFullNames.WithNotifyOnDescendants(
                out XElement adHoc,
                onPC: (sender, e) => { },
                onCC: (sender, e) => { },
                options: ModelingOption.CachePropertyInfo | ModelingOption.ShowFullNameForTypes
            );

            actual = adHoc.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)XBoundObjectMSTest.TestClassesForModeling.Common.ClassA"" instance=""[XBoundObjectMSTest.TestClassesForModeling.Common.ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" pi=""[System.Int32]"" />
  <member name=""BCollection"" pi=""[System.Collections.ObjectModel.ObservableCollection]"" instance=""[System.Collections.ObjectModel.ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"">
    <member name=""Count"" pi=""[System.Int32]"" />
  </member>
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting expecting full names for types"
            );
        }

        // EXPECT
        // - An instance of ClassB is added to BCollection.
        // - A NotifyCollectionChanged event is triggered with action `Add`.
        // - A PropertyChanged event is triggered for the `Count` property of BCollection.
        // - The new ClassB instance appears in the model within BCollection.
        // - ClassB contains an instance of ClassC with observable properties Cost and Currency.
        void subtestAddClassBThenViewModel()
        {
            clearQueues();
            A.BCollection.Add(new());

            // Inspect CC event
            currentEvent = eventsCC.DequeueSingle();
            Assert.AreEqual(
                NotifyCollectionChangedAction.Add,
                currentEvent.NotifyCollectionChangedEventArgs?.Action,
                "Expecting response to item added.");
            { }
            // Inspect PC event
            currentEvent = eventsPC.DequeueSingle();
            Assert.AreEqual(
                nameof(IList.Count),
                currentEvent.PropertyChangedEventArgs?.PropertyName,
                "Expecting response to BCollection count changed.");

            actual = currentEvent.OriginModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" pi=""[Int32]"" />
  <member name=""BCollection"" pi=""[ObservableCollection]"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"">
    <member name=""Count"" pi=""[Int32]"" />
    <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"">
      <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"">
        <member name=""Name"" pi=""[String]"" />
        <member name=""Cost"" pi=""[Int32]"" />
        <member name=""Currency"" pi=""[Int32]"" />
      </member>
    </model>
  </member>
</model>";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting response to BCollection count changed."
            );
        }

        // EXPECT
        // - The Cost property in ClassC is updated.
        // - A PropertyChanged event is triggered for the Cost property.
        // - The TotalCost property of ClassA is recalculated accordingly.
        void subtestExerciseNestedCostProperty()
        {
            var classB = A.BCollection.First();
            classB.C.Cost = rando.Next(Int16.MaxValue); // Property change
            currentEvent = eventsPC.DequeueSingle();
            actual = currentEvent.SenderModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<member name=""Cost"" pi=""[Int32]"" />";
            // Is A.TotalCost updated?
            Assert.AreEqual(
                8148,
                A.TotalCost,
                "Expecting that A has recalculated Total Cost");
        }

        // EXPECT
        // - A new instance of ClassB is created and assigned an initial Cost value.
        // - The instance is added to BCollection.
        // - The TotalCost property of ClassA is updated to reflect the new cost.
        void subtestAddClassBInstanceWithNonZeroInitialCost()
        {
            var newClassB = new ClassB();
            newClassB.C.Cost = rando.Next(Int16.MaxValue);
            A.BCollection.Add(newClassB);

            Assert.AreEqual(
                11776,
                A.TotalCost,
                "Expecting an updated value from the addition of ClassB");
        }

        // EXPECT
        // - Three new instances of ClassB are created and added to BCollection.
        // - A total of five ClassB instances should now be visible in the model.
        // - Three NotifyCollectionChanged events with action `Add` are triggered.
        // - Three PropertyChanged events for `Count` are triggered as BCollection updates.
        // - Each added ClassB contains a ClassC instance with observable properties Cost and Currency.
        // - Updating the Cost property in each new ClassC instance triggers five cost updates.
        // - The cumulative TotalCost value should be updated accordingly.
        void subtestAdd3xClassBInstanceWithZeroInitialCost()
        {
            clearQueues();
            var classBx3 =
                Enumerable.Range(0, 3)
                .Select(_ => new ClassB())
                .ToList();
            classBx3
                .ForEach(_ => A.BCollection.Add(_));

            actual = A.OriginModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" pi=""[Int32]"" />
  <member name=""BCollection"" pi=""[ObservableCollection]"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"">
    <member name=""Count"" pi=""[Int32]"" />
    <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"">
      <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"">
        <member name=""Name"" pi=""[String]"" />
        <member name=""Cost"" pi=""[Int32]"" />
        <member name=""Currency"" pi=""[Int32]"" />
      </member>
    </model>
    <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"">
      <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"">
        <member name=""Name"" pi=""[String]"" />
        <member name=""Cost"" pi=""[Int32]"" />
        <member name=""Currency"" pi=""[Int32]"" />
      </member>
    </model>
    <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"">
      <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"">
        <member name=""Name"" pi=""[String]"" />
        <member name=""Cost"" pi=""[Int32]"" />
        <member name=""Currency"" pi=""[Int32]"" />
      </member>
    </model>
    <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"">
      <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"">
        <member name=""Name"" pi=""[String]"" />
        <member name=""Cost"" pi=""[Int32]"" />
        <member name=""Currency"" pi=""[Int32]"" />
      </member>
    </model>
    <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"">
      <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"">
        <member name=""Name"" pi=""[String]"" />
        <member name=""Cost"" pi=""[Int32]"" />
        <member name=""Currency"" pi=""[Int32]"" />
      </member>
    </model>
  </member>
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting values to match."
            );
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting a total of 5 items visible in the model."
            );
            Assert.AreEqual(
                3,
                eventsCC.Count,
                "Expecting 3x Action.Add CC events in this round.");
            joined = string.Join(",", eventsCC.Select(_ => _.NotifyCollectionChangedEventArgs?.Action));
            Assert.AreEqual("Add,Add,Add", joined);

            joined = string.Join(",", eventsPC.Select(_ => _.PropertyChangedEventArgs?.PropertyName));
            Assert.AreEqual("Count,Count,Count", joined);
            Assert.AreEqual(
                3,
                eventsCC.Count,
                "Expecting 3x PropertyChanged PC events (where BCollection.Count changes.");
            clearQueues();

            classBx3
                .ForEach(_ => _.C.Cost = rando.Next(Int16.MaxValue));

            joined = string.Join(
                Environment.NewLine,
                eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args));

            actual = joined;
            expected = @" 
Item at index 0 shows a new cost value of 8148.
Total of C.Cost 8148
Item at index 1 shows a new cost value of 3628.
Total of C.Cost 11776
Item at index 2 shows a new cost value of 15302.
Total of C.Cost 27078
Item at index 3 shows a new cost value of 25283.
Total of C.Cost 52361
Item at index 4 shows a new cost value of 21544.
Total of C.Cost 73905";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 5 consistent pseudorandom cost updates in total."
            );
        }

        // EXPECT
        // - The last item in BCollection is retrieved and removed.
        // - A NotifyCollectionChanged event with action `Remove` is triggered.
        // - Unsubscribe events are processed for the removed item and its dependencies.
        // - Further property changes on the removed instance do not trigger PropertyChanged events.
        void subtestRemoveLastItemAndVerifyUnsubscribe()
        {
            clearQueues(ClearQueue.All);

            // Get the last item
            var remove = A.BCollection.Last();
            // Remove it
            A.BCollection.Remove(remove);

            Assert.AreEqual(
                NotifyCollectionChangedAction.Remove,
                eventsCC.DequeueSingle().NotifyCollectionChangedEventArgs?.Action);
            clearQueues();

            var joined = string.Join(
                Environment.NewLine,
                eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args?.ToString()));

            actual = joined;

            actual.ToClipboard();
            actual.ToClipboardAssert();

            expected = @" 
Removing INPC Subscription
Remove <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting OnAwaited hooks for removal events."
            );
            clearQueues(ClearQueue.All);
            remove.C.Cost = rando.Next(Int16.MaxValue);
            Assert.AreEqual(0, eventsPC.Count, "Expecting successful unsubscribe");
        }

        // EXPECT
        // - Iterates through all descendants of A.OriginModel that contain ClassC.
        // - Computes the total cost by summing up the Cost property of each ClassC instance.
        // - Validates that the expected total cost is 73905.
        void subtestTryIteratingForClassC()
        {
            var totalCost = 0;
            // Long form
            foreach (XElement desc in A.OriginModel.Descendants().Where(_ => _.Has<ClassC>()))
            {
                totalCost += desc.To<ClassC>().Cost;
            }
            Assert.AreEqual(73905, totalCost);

            // Short form
            totalCost =
                A
                .OriginModel
                .Descendants()
                .Where(_ => _.Has<ClassC>())
                .Sum(_ => _.To<ClassC>().Cost);
            Assert.AreEqual(73905, totalCost);
        }

        // EXPECT
        // - All items in BCollection are retrieved and then removed using Clear().
        // - The model should reflect the empty state of BCollection.
        // - Multiple unsubscribe events should be triggered for each removed instance.
        // - Further property changes on the removed instances do not trigger PropertyChanged events.
        void subtestClearListAndVerifyUnsubscribe()
        {
            var removes = A.BCollection.ToArray();
            clearQueues(ClearQueue.All);
            A.BCollection.Clear();

            actual = A.OriginModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" pi=""[Int32]"" />
  <member name=""BCollection"" pi=""[ObservableCollection]"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" />
</model>";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting origin model reflects removal."
            );

            var joined = string.Join(
                Environment.NewLine,
                eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args?.ToString()));

            actual = joined;
            expected = @" 
Removing INPC Subscription
Remove <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <member name=""C"" pi=""[ClassC]"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting comprehensive unsubscribe based on XObject events and OnAwaited hook."
            );

            clearQueues(ClearQueue.All);
            foreach (var remove in removes)
            {
                remove.C.Cost = rando.Next(Int16.MaxValue);
            }
            Assert.AreEqual(0, eventsPC.Count, "Expecting successful unsubscribe");
        }

        #endregion S U B T E S T S
    }

    /// <summary>
    /// Unit test for verifying the event-driven updates and sum calculations in an ObservableCollection<ClassB>.
    /// This test ensures that:
    /// 1. Property changes in `ClassC.Cost` or swapping of `ClassB.C` within `BCollection` triggers appropriate updates.
    /// 2. The formatted summary of cost calculations is built and updated correctly.
    /// 3. Randomly setting `ClassC.Cost` results in accurate and expected summations and outputs.
    /// 4. Replacing `ClassC` instances results in updated cost calculations and confirms the propagation of change events.
    /// </summary>

    [TestMethod]
    public void Test_SO_79467031_5438626_BCollection()
    {
        var builder = new List<string>();
        string joined;
        int autoIncrement = 1;
        int SumOfBCost = 0;
        ObservableCollection<ClassB>? BCollection = null;
        BCollection = new ObservableCollection<ClassB>
        {
            new ClassB{C = new ClassC{Name = $"Item C{autoIncrement++}"} },
            new ClassB{C = new ClassC{Name = $"Item C{autoIncrement++}"} },
            new ClassB{C = new ClassC{Name = $"Item C{autoIncrement++}"} },
        }.WithNotifyOnDescendants(OnPropertyChanged);

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            eventsPC.Enqueue(new SenderEventPair(sender, e));
            switch (e.PropertyName)
            {
                case nameof(ClassC.Cost):   // Cost changed
                case nameof(ClassB.C):      // The 'C' instance swapped out
                    SumOfBCost = BCollection?.Sum(_ => _.C?.Cost ?? 0) ?? 0;
                    var values = BCollection?.Select(_ => (_.C?.Cost ?? 0).ToString());
                    builder.Add($"{string.Join(" + ", values ?? [])} = {SumOfBCost}");
                    break;
                default:
                    break;
            }
        }
        // Set of three.
        BCollection[0].C.Cost = rando.Next(Int16.MaxValue);
        BCollection[1].C.Cost = rando.Next(Int16.MaxValue);
        BCollection[2].C.Cost = rando.Next(Int16.MaxValue);

        joined = string.Join(Environment.NewLine, builder);
        actual = joined;
        actual.ToClipboard();
        actual.ToClipboardAssert("Expecting SumOfBCost updates.");
        { }
        expected = @" 
8148 + 0 + 0 = 8148
8148 + 3628 + 0 = 11776
8148 + 3628 + 15302 = 27078";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting SumOfBCost updates."
        );
        builder.Clear();

        localOnTestReplaceCObjects();

        joined = string.Join(Environment.NewLine, builder);
        actual = joined;
        actual.ToClipboard();
        actual.ToClipboardAssert("Expecting 3 replacement values of zero.");
        { }
        expected = @" 
0 + 3628 + 15302 = 18930
0 + 0 + 15302 = 15302
0 + 0 + 0 = 0";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting SumOfBCost updates."
        );
        builder.Clear();

        // We have PC events from ClassB.C changing, so flush them.
        clearQueues();

        // Set of three.
        BCollection[0].C.Cost = rando.Next(Int16.MaxValue);
        BCollection[1].C.Cost = rando.Next(Int16.MaxValue);
        BCollection[2].C.Cost = rando.Next(Int16.MaxValue);

        joined = string.Join(Environment.NewLine, builder);
        actual = joined;
        actual.ToClipboard();
        actual.ToClipboardAssert("Expecting relacement objects are still firing events.");
        { }
        expected = @" 
25283 + 0 + 0 = 25283
25283 + 21544 + 0 = 46827
25283 + 21544 + 14180 = 61007";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting relacement objects are still firing events."
        );
        builder.Clear();

        void localOnTestReplaceCObjects()
        {
            int replaceIndex = 1;
            foreach (ClassB classB in BCollection)
            {
                classB.C = new ClassC { Name = $"Replace C{replaceIndex++}" };
            }
        }
    }

    [TestMethod]
    public void Test_IterationBasics()
    {
        ClassA classA = new();
        // Add 3 ClassB instances to classA;
        Enumerable
            .Range(0, 3)
            .ToList()
            .ForEach(_ => classA.BCollection.Add(new()));
        // EXPECT
        // - XML Model where only Reference types are bound to instance attribute.
        subtestGenerateBareMetalModel();
        // EXPECT
        // - Yield return on the ModelDescendantsAndSelf enumerator.
        subtestDiscoveryEnumerator();
        // EXPECT
        // - XML Model where value types are also bound as instance attribute.
        subtestIncludeValueTypeInstances();

        #region S U B T E S T S
        void subtestGenerateBareMetalModel()
        {
            XElement originModel =
                classA
                .ModelDescendantsAndSelf(null)
                .ToArray()
                .First();

            actual = originModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" />
  <member name=""BCollection"" instance=""[ObservableCollection]"">
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <member name=""Count"" />
  </member>
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting values to match."
            );
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model to match."
            );
            originModel.RemoveAll();
        }
        void subtestDiscoveryEnumerator()
        {
            ModelingContext context = new();
            var builder = new List<string>();
            foreach (var xel in classA.ModelDescendantsAndSelf(context))
            {
                string tabs = string.Join(string.Empty, Enumerable.Repeat("  ", xel.Ancestors().Count()));
                builder.Add($"{tabs}{xel.ToShallow().SortAttributes<SortOrderNOD>()}");
            }
            var joined = string.Join(Environment.NewLine, builder);
            actual = joined;

            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"" />
  <member name=""TotalCost"" />
  <member name=""BCollection"" instance=""[ObservableCollection]"" />
    <model instance=""[ClassB]"" />
      <member name=""C"" instance=""[ClassC]"" />
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
    <model instance=""[ClassB]"" />
      <member name=""C"" instance=""[ClassC]"" />
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
    <model instance=""[ClassB]"" />
      <member name=""C"" instance=""[ClassC]"" />
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
    <member name=""Count"" />";
                Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting each element has yielded a shallow representation."
            );

            actual = context.OriginModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" />
  <member name=""BCollection"" instance=""[ObservableCollection]"">
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <member name=""Count"" />
  </member>
</model>";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting origin model format"
            );

            // Attempt a refresh of the BCollection
            var addedModel = classA.CreateModel(context.Clone());

            actual = addedModel.ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"">
  <member name=""TotalCost"" />
  <member name=""BCollection"" instance=""[ObservableCollection]"">
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" />
        <member name=""Cost"" />
        <member name=""Currency"" />
      </member>
    </model>
    <member name=""Count"" />
  </member>
</model>";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting target model format (i.e. without the [ModelingContext] xba)."
            );
        }
        void subtestIncludeValueTypeInstances()
        {
            var context = new ModelingContext()
            {
                Options = ModelingOption.IncludeValueTypeInstances,
            };

            actual = classA.CreateModel(context).ToString();
            expected = @" 
<model name=""(Origin)ClassA"" instance=""[ClassA]"" context=""[ModelingContext]"">
  <member name=""TotalCost"" instance=""[Int32]"" />
  <member name=""BCollection"" instance=""[ObservableCollection]"">
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" instance=""[String]"" />
        <member name=""Cost"" instance=""[Int32]"" />
        <member name=""Currency"" instance=""[Int32]"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" instance=""[String]"" />
        <member name=""Cost"" instance=""[Int32]"" />
        <member name=""Currency"" instance=""[Int32]"" />
      </member>
    </model>
    <model instance=""[ClassB]"">
      <member name=""C"" instance=""[ClassC]"">
        <member name=""Name"" instance=""[String]"" />
        <member name=""Cost"" instance=""[Int32]"" />
        <member name=""Currency"" instance=""[Int32]"" />
      </member>
    </model>
    <member name=""Count"" instance=""[Int32]"" />
  </member>
</model>";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting value type instances"
            );
        }
        #endregion S U B T E S T S
    }

    // EXPECT
    // - The ObservableCollection is initialized with four ABC instances.
    // - CRITICAL: The origin model should correctly reflect all pre-existing instances in the collection.
    // - Enumerating the collection should NOT trigger any events.
    // - The collection's ToString() override should return the expected formatted string representation.

    [TestMethod]
    public void Test_ObservableCollectionNOD()
    {
        XElement originModel;
        Dictionary<Enum, int> eventDict = new();
        var obc = new ObservableCollection<ABC>
            {
                new ABC(),
                new ABC(),
                new ABC(),
                new ABC(),
            }.WithNotifyOnDescendants(
            out originModel,
            onPC: (sender, e) => eventsPC.Enqueue(new SenderEventPair(sender, e)),
            onCC: (sender, e) => eventsCC.Enqueue(new SenderEventPair(sender, e)));


        // ====================================================================
        // Use the ToString() override of class ABC to show the contents of OBC.
        // ====================================================================

        var joined = string.Join(Environment.NewLine, obc);
        actual = joined;
        actual.ToClipboard();
        expected = @" 
A | B | C
A | B | C
A | B | C
A | B | C";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting ToString representation of 4 ABC items"
        );

        Assert.AreEqual(0, eventDict.Count, "Expecting no events yet");

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        actual.ToClipboard();
        actual.ToClipboardAssert("Expecting origin model to match");
        { }
        expected = @" 
<model name=""(Origin)ObservableCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" context=""[ModelingContext]"">
  <member name=""Count"" />
  <model name=""(Origin)ABC"" instance=""[ABC]"" onpc=""[OnPC]"">
    <member name=""A"" runtimetype=""[String]"" />
    <member name=""B"" runtimetype=""[String]"" />
    <member name=""C"" runtimetype=""[String]"" />
  </model>
  <model name=""(Origin)ABC"" instance=""[ABC]"" onpc=""[OnPC]"">
    <member name=""A"" runtimetype=""[String]"" />
    <member name=""B"" runtimetype=""[String]"" />
    <member name=""C"" runtimetype=""[String]"" />
  </model>
  <model name=""(Origin)ABC"" instance=""[ABC]"" onpc=""[OnPC]"">
    <member name=""A"" runtimetype=""[String]"" />
    <member name=""B"" runtimetype=""[String]"" />
    <member name=""C"" runtimetype=""[String]"" />
  </model>
  <model name=""(Origin)ABC"" instance=""[ABC]"" onpc=""[OnPC]"">
    <member name=""A"" runtimetype=""[String]"" />
    <member name=""B"" runtimetype=""[String]"" />
    <member name=""C"" runtimetype=""[String]"" />
  </model>
</model>";
        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting origin model to match"
        );

        // ====================================================================
        // We are REPLACING the FIRST LIST ITEM in its entirity.
        // ====================================================================
        clearQueues();
        obc[0] = new ABC();

        currentEvent = eventsCC.DequeueSingle();

        Assert.AreEqual(
            NotifyCollectionChangedAction.Replace,
            currentEvent.NotifyCollectionChangedEventArgs.Action
        );

        // =====================================================================
        // NOW make sure the new instance has successfully bound PropertyChanges.
        // =====================================================================

        obc[0].A = "AA";    // Property change
        currentEvent = eventsPC.DequeueSingle();
        Assert.AreEqual(
            nameof(ABC.A),
            currentEvent.PropertyName,
            "Expecting property changed event has been raised.");
    }

    [TestMethod]
    public void Test_XObjectChangeDelegate()
    {
        List<string> builder = new();
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            builder.Add($"{e.Args}");
        }
        try
        {
            Awaited += localOnAwaited;
            string joined;
            var classA = new ClassA(false);
            _ = classA.WithNotifyOnDescendants(
                out XElement model,
                onPC: (sender, e) =>
                {
                    eventsPC.Enqueue(new SenderEventPair(sender, e));
                },
                onCC: (sender, e) =>
                {
                    eventsCC.Enqueue(new SenderEventPair(sender, e));
                },
                onXO: (sender, e) =>
                {
                    if (e is XObjectChangedOrChangingEventArgs ePlus && !ePlus.IsChanging)
                    {
                        builder.Add(ePlus.ToString(sender, timestamp: false) ?? "Error");
                    }
                });

            joined = string.Join(Environment.NewLine, builder);
            actual = joined;
            expected = @" 
Add    XAttribute Changed : name   
Add    XAttribute Changed : instance 
Add    XElement   Changed : member TotalCost
Add    XElement   Changed : member BCollection
Add    XAttribute Changed : instance 
Add    XElement   Changed : member Count
Add    XAttribute Changed : onpc   
Added INPC Subscription
<member name=""BCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" />
Add    XAttribute Changed : oncc   
Added INCC Subscription
<member name=""BCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" />";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting un-timestamped message reporting"
            );
            builder.Clear();

            classA.BCollection.Add(new());

            joined = string.Join(Environment.NewLine, builder);
            actual = joined;

            actual.ToClipboard();
            actual.ToClipboardAssert();
            expected = @" 
Added INPC Subscription
<model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""C"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Add    XElement   Changed : model  (Origin)ClassB";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting un-timestamped message reporting"
            );
            builder.Clear();
            classA.BCollection[0] = new();

            joined = string.Join(Environment.NewLine, builder);
            actual = joined;
            expected = @" 
Remove XElement   Changed : member Currency
Remove XElement   Changed : member Cost
Remove XElement   Changed : member Name
Removing INPC Subscription
Remove <member name=""C"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Remove XElement   Changed : member C
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Remove XElement   Changed : model  (Origin)ClassB
Added INPC Subscription
<model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""C"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Add    XElement   Changed : model  (Origin)ClassB";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting subscription removal"
            );
        }
        finally
        {
            Awaited -= localOnAwaited;
        }
    }

    [TestMethod]
    public void Test_INPCwithINPCs()
    {
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        string actual, expected, joined;
        XElement originModel;
        ObservableCollection<INPCwithINPCs> obc;

        subtestInstancesWithNullMemberPropertiesABC1andABC2();
        subtestAssignABC1InstanceVerifyINPC();
        subtestReturnABC1ToNull();
        subtestOBCReset();
        subtestInstancesWithNotNullMemberPropertiesABC1andABC2();

        subtestSetObjectPropertyToValueType();

        #region S U B T E S T S
        void subtestInstancesWithNullMemberPropertiesABC1andABC2()
        {
            obc = new ObservableCollection<INPCwithINPCs>()
            {
                new INPCwithINPCs (),
                new INPCwithINPCs (),
            }.WithNotifyOnDescendants(
                out originModel,
                onPC: (sender, e) => eventsPC.Enqueue(new SenderEventPair(sender, e)),
                onCC: (sender, e) => eventsCC.Enqueue(new SenderEventPair(sender, e)),
                options: ModelingOption.CachePropertyInfo);

            actual = originModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ObservableCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" context=""[ModelingContext]"">
  <member name=""Count"" pi=""[Int32]"" />
  <model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"">
    <member name=""ABC1"" pi=""[ABC]"" />
    <member name=""ABC2"" pi=""[ABC]"" />
  </model>
  <model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"">
    <member name=""ABC1"" pi=""[ABC]"" />
    <member name=""ABC2"" pi=""[ABC]"" />
  </model>
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ABC1 and ABC2 to default to NULL."
            );
        }

        void subtestAssignABC1InstanceVerifyINPC()
        {
            // CRITICAL BEHAVIOR !
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // ABC new instance must come up connected!
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            clearQueues(ClearQueue.OnAwaitedEvents);
            obc[0].ABC1 = new ABC();
            currentEvent = eventsPC.DequeueSingle();

            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // What Just Happened:
            // - ABC1 just went from null status of WaitingForValue to
            //   a new state of having a model and running discovery on it.
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            // EXPECT
            // - There should be 'runtimetype' attributes because the declared property is different (i.e. 'object').
            // - The ABC1 property model should be populated and no longer 'WaitingForValue'.
            actual =
                currentEvent.SenderModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"">
  <member name=""A"" pi=""[Object]"" runtimetype=""[String]"" />
  <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
  <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
</member>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting property changed for ABC1"
            );
            Assert.AreEqual(
                currentEvent.PropertyName,
                nameof(INPCwithINPCs.ABC1),
                "Expecting ABC1 is modeled as a NON-NULL instance");
            Assert.IsNotNull(currentEvent.PropertyChangedEventArgs);


            joined = string.Join(Environment.NewLine, eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args?.ToString()));
            actual = joined;
            expected = @" 
Added INPC Subscription
<member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ABC1 subscription OA."
            );
        }

        void subtestReturnABC1ToNull()
        {
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // Set back to null
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            // CRITICAL BEHAVIOR !
            clearQueues(ClearQueue.OnAwaitedEvents);
            obc[0].ABC1 = null;
            currentEvent = eventsPC.DequeueSingle();
            actual =
                currentEvent.SenderModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<member name=""ABC1"" pi=""[ABC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ABC1 is modeled as a NULL instance"
            );

            joined = string.Join(Environment.NewLine, eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args?.ToString()));
            actual = joined;
            expected = @" 
Removing INPC Subscription
Remove <member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting unsubscription"
            );
        }

        void subtestOBCReset()
        {
            clearQueues(ClearQueue.OnAwaitedEvents);
            obc.Clear();

            actual = originModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ObservableCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" context=""[ModelingContext]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting model shows empty observable collection."
            );

            joined = string.Join(Environment.NewLine, eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args?.ToString()));
            actual = joined;
            expected = @" 
Removing INPC Subscription
Remove <model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"" />
Removing INPC Subscription
Remove <model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting unsubscribe 2x OAs."
            );
        }    

        void subtestInstancesWithNotNullMemberPropertiesABC1andABC2()
        {
            clearQueues(ClearQueue.OnAwaitedEvents);
            // Instances where ABC1 and ABC2 are instantiated to begin with.
            obc.Add(new() { ABC1 = new(), ABC2 = new(), });
            obc.Add(new() { ABC1 = new(), ABC2 = new(), });

            actual = originModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ObservableCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" context=""[ModelingContext]"">
  <model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"">
    <member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
    </member>
    <member name=""ABC2"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
    </member>
  </model>
  <model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"">
    <member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
    </member>
    <member name=""ABC2"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
    </member>
  </model>
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting NON-NULL property instances."
            );

            joined = string.Join(Environment.NewLine, eventsOA.Select(_ => (_.e as AwaitedEventArgs)?.Args?.ToString()));
            actual = joined;
            expected = @" 
Added INPC Subscription
<model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""ABC2"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"" />
Added INPC Subscription
<model name=""(Origin)INPCwithINPCs"" instance=""[INPCwithINPCs]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""ABC1"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""ABC2"" pi=""[ABC]"" instance=""[ABC]"" onpc=""[OnPC]"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 6x subscriptions"
            );
        }       

        void subtestSetObjectPropertyToValueType()
        {
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // Change a value that is NOT an INPC model
            // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            var abc1 = obc[0].ABC1 ?? throw new NullReferenceException();
            clearQueues();
            abc1.A = 1; // Property Change
            currentEvent = eventsPC.DequeueSingle();

            actual = currentEvent.SenderModel?.ToString() ?? throw new NullReferenceException();
            expected = @" 
<member name=""A"" pi=""[Object]"" runtimetype=""Int32"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting values to match."
            );
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 'A' now has Int32 runtime type because its value is 1."
            );
        }
        #endregion S U B T E S T S
    }

    /// <summary>
    /// This test verifies that the Lazy<T> proxy correctly triggers notifications 
    /// for property changes, allowing the model to run discovery.
    /// 
    /// Problem: The Lazy<T> implementation does not provide notifications when 
    ///          its value is created or updated.
    /// 
    /// Solution: By utilizing a notification mechanism, this test ensures that 
    ///           property change events are properly raised, allowing discovery 
    ///           to detect changes in singleton instances.
    ///           
    /// Test Flow:
    /// 1. Initialize a ValidSingletonTestModel with notification handlers.
    /// 2. Validate that the model structure is as expected before activation.
    /// 3. Trigger property access and confirm notifications fire correctly.
    /// 4. Verify that Lazy<T> watcher mechanism allows property changes to be detected.
    /// 5. Ensure that the model updates only after the LazyProxy expires and fires a change.
    /// </summary>
    [TestMethod]
    public void Test_Singleton()
    {
        object @lock = new();
        Stopwatch stopwatch = new Stopwatch();

        ClassWithSingletonProperty cwsp =
            new ClassWithSingletonProperty()
            .WithNotifyOnDescendants(
                out XElement originModel,
                onPC: (sender, e) =>
                {
                    eventsPC.Enqueue(new SenderEventPair(sender, e));
                },
                onCC: (sender, e) => eventsCC.Enqueue(new SenderEventPair(sender, e)), 
                options: ModelingOption.CachePropertyInfo
            );
        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithSingletonProperty"" instance=""[ClassWithSingletonProperty]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""ABC1IsValueCreated"" pi=""[Boolean]"" />
  <member name=""ABC1"" pi=""[INotifyPropertyChanged]"" />
</model>";
        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting discovery without activating singleton."
        );

        clearQueues();
        Assert.IsFalse(cwsp.ABC1IsValueCreated);
        var abc1 = (ABC)cwsp.ABC1;
        Assert.IsTrue(cwsp.ABC1IsValueCreated);
        currentEvent = eventsPC.DequeueSingle();
        Assert.AreEqual(
            nameof(ClassWithSingletonProperty.ABC1),
            currentEvent.PropertyName,
            "Expecting property change when singleton is activated"
        );

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithSingletonProperty"" instance=""[ClassWithSingletonProperty]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""ABC1IsValueCreated"" pi=""[Boolean]"" />
  <member name=""ABC1"" pi=""[INotifyPropertyChanged]"" instance=""[ABC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" runtimetype=""[String]"" />
    <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
    <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting model creation for new singleton instance."
        );
    }

    [TestMethod]
    public void Test_ModelInit()
    {
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        string actual, expected;
        XElement? model = null;

        // =========================================================
        // Empty class with no available changed events.
        // =========================================================
        new EmptyClass()
            .WithNotifyOnDescendants(out model, OnPropertyChanged, OnCollectionChanged);

        actual = model.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)EmptyClass"" instance=""[EmptyClass]"" context=""[ModelingContext]"" />";
       
        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting no event handlers for this instance"
        );

        // =========================================================
        // Empty class with available INPC
        // =========================================================
        _ = new EmptyOnNotifyClass()
            .WithNotifyOnDescendants(out model, OnPropertyChanged, OnCollectionChanged);

        actual = model.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)EmptyOnNotifyClass"" instance=""[EmptyOnNotifyClass]"" onpc=""[OnPC]"" context=""[ModelingContext]"" />";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting INPC detected on class with no child properties."
        );

        // =========================================================
        // Observable Collection
        // =========================================================
        _ = new ObservableCollection<object>()
            .WithNotifyOnDescendants(out model, OnPropertyChanged, OnCollectionChanged);

        actual = model.SortAttributes<SortOrderNOD>().ToString();

        expected = @" 
<model name=""(Origin)ObservableCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" context=""[ModelingContext]"">
  <member name=""Count"" />
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting INPC and INCC both detected on ObservableCollection T."
        );
    }


    [TestMethod]
    public void Test_ObservableCollectionSolo()
    {
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"{e.GetType().Name} {e.PropertyName}");
            eventsPC.Enqueue(new SenderEventPair(sender, e));
        }
        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            eventsCC.Enqueue(new SenderEventPair(sender, e));
        }

        var oc =
            new ObservableCollection<ABC>()
            .WithNotifyOnDescendants(out XElement originModel, OnPropertyChanged, OnCollectionChanged);


        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ObservableCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" context=""[ModelingContext]"">
  <member name=""Count"" />
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting model"
        );
        oc.Add(new());

        // Thows an exception and fails the test if event wasn't received.
        currentEvent = eventsCC.DequeueSingle();
    }

    [TestMethod]
    public void Test_SetNullPropertyToString()
    {
        XElement originModel;
        ClassWithOnePropertyINPC 
            cwopLevel0;

        subtestClassWithSingleObjectDefaultNullProperty();

        subtestLookForPropertyChangedWhenPropertyIsSet();


        #region S U B T E S T S 
        void subtestClassWithSingleObjectDefaultNullProperty()
        {
            cwopLevel0 =
                new ClassWithOnePropertyINPC()
                .WithNotifyOnDescendants(
                    out originModel,
                    onPC: (sender, e) => eventsPC.Enqueue(new SenderEventPair(sender, e)),
                    onCC: (sender, e) => eventsCC.Enqueue(new SenderEventPair(sender, e)),
                    options: ModelingOption.CachePropertyInfo);
            // Loopback
            _ = originModel.To<ClassWithOnePropertyINPC>(@throw: true);


            actual = originModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting null instance of System.Object"
            );
        }
        

        void subtestLookForPropertyChangedWhenPropertyIsSet()
        {
            // =========================================================
            // Setting 'A' to an instance come with certain expectations.
            // - Doing this must raise a PropertyChanged event.
            // =========================================================

            cwopLevel0.A = Guid.NewGuid().ToString(); // Property Change

            currentEvent = eventsPC.Dequeue();
            Assert.AreEqual(0, eventsPC.Count);
            Assert.AreEqual(1, currentEvent.SenderModel?.Ancestors().Count());

            actual = originModel.SortAttributes<SortOrderNOD>().ToString();
            expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" runtimetype=""String"" />
</model>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting origin model to match"
            );
            // Look at PROPERTY model
            actual = currentEvent.SenderModel.SortAttributes<SortOrderNOD>().ToString() ?? throw new NullReferenceException();
            expected = @" 
<member name=""A"" pi=""[Object]"" runtimetype=""String"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting refreshed property model"
            );
        }
        #endregion S U B T E S T S 

        // =========================================================
        // Setting 'A' back to null
        // =========================================================

        cwopLevel0.A = null; // Property Change

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" />
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting INPCSource"
        );
        // =========================================================
        // Setting 'A'to a new INPC instance at level 1
        // =========================================================

        cwopLevel0.A = new ClassWithOnePropertyINPC(); // Property Change

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();

        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" />
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting INPCSource"
        );
        Assert.AreEqual(1, currentEvent.SenderModel.Ancestors().Count());

        object? objectLevel1 =
            currentEvent.SenderModel.Attribute(nameof(SortOrderNOD.instance)) is XBoundAttribute xba
            ? xba.Tag
            : null;
        Assert.AreEqual(objectLevel1?.GetType(), typeof(ClassWithOnePropertyINPC));

        // =========================================================
        // Setting 'A' on the leaf instance to a string
        // =========================================================

        var cwopLevel1 = (ClassWithOnePropertyINPC)objectLevel1;

        cwopLevel1.A = Guid.NewGuid().ToString(); // Property Change

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);

        Assert.AreEqual(2, currentEvent.SenderModel?.Ancestors().Count());

        actual = originModel.SortAttributes<SortOrderNOD>().ToString() ?? throw new NullReferenceException();
        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" runtimetype=""String"" />
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting model is RELATIVE TO SENDER AT LEVEL 1"
        );

        // =========================================================
        // GETTING READY TO FOR UNSUBSCRIBE TESTS - first make sure
        // we've got at least two INPS under the main one.
        // =========================================================

        var leafABC = new ABC
        {
            A = new EmptyClass()
        };

        cwopLevel1.A = leafABC; // [Careful] THIS IS RIGHT. It's a change on level 1. Property Change

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);
        Assert.AreEqual(2, currentEvent.SenderModel?.Ancestors().Count());

        Assert.ReferenceEquals(
            cwopLevel1,
            currentEvent!
            .SenderModel?
            .Parent?
            .To<ClassWithOnePropertyINPC>(@throw: true));

        var abcLevel2 = currentEvent.SenderModel.To<ABC>(@throw: true);

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" instance=""[EmptyClass]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""[String]"" />
      <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
    </member>
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting to see all layers in their glory."
        );

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++ P R O P E R T Y    C H A N G E   ++++++++++++++++++++++++
        abcLevel2.B = 250309; // 15 years!

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);
        Assert.AreEqual(3, currentEvent.SenderModel?.Ancestors().Count());

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" instance=""[EmptyClass]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""Int32"" />
      <member name=""C"" pi=""[Object]"" runtimetype=""[String]"" />
    </member>
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting origin model to match"
        );

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++ P R O P E R T Y    C H A N G E   ++++++++++++++++++++++++
        abcLevel2.C = new EmptyOnNotifyClass();

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);

        Assert.AreEqual(3, currentEvent.SenderModel.Ancestors().Count());

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" instance=""[EmptyClass]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""Int32"" />
      <member name=""C"" pi=""[Object]"" instance=""[EmptyOnNotifyClass]"" onpc=""[OnPC]"" />
    </member>
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting origin model to match."
        );

        // =========================================================
        // NOW READY TO FOR UNSUBSCRIBE TESTS
        // =========================================================

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++ P R O P E R T Y    C H A N G E   ++++++++++++++++++++++++
        abcLevel2.C = null!;

        currentEvent = eventsPC.Dequeue();
        Assert.AreEqual(0, eventsPC.Count);
        Assert.AreEqual(3, currentEvent.SenderModel.Ancestors().Count());

        actual = originModel.SortAttributes<SortOrderNOD>().ToString();
        expected = @" 
<model name=""(Origin)ClassWithOnePropertyINPC"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"" context=""[ModelingContext]"">
  <member name=""A"" pi=""[Object]"" instance=""[ClassWithOnePropertyINPC]"" onpc=""[OnPC]"">
    <member name=""A"" pi=""[Object]"" instance=""[ABC]"" onpc=""[OnPC]"">
      <member name=""A"" pi=""[Object]"" instance=""[EmptyClass]"" />
      <member name=""B"" pi=""[Object]"" runtimetype=""Int32"" />
      <member name=""C"" pi=""[Object]"" />
    </member>
  </member>
</model>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting abcLevel2.C is now null."
        );
    }
}
