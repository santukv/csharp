namespace sodium.tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class BehaviorTester : SodiumTestCase
    {
        [Test]
        public void TestHold()
        {
            var evt = new EventSink<Int32>();
            var behavior = evt.Hold(0);
            var results = new List<Int32>();
            var listener = behavior.Updates().Listen(results.Add);
            evt.Send(2);
            evt.Send(9);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(2, 9), results);
        }

        [Test]
        public void TestSnapshot()
        {
            var behavior = new BehaviorSink<Int32>(0);
            var evt = new EventSink<Int64>();
            var results = new List<String>();
            var snapshotFunction = new BinaryFunction<long, int, string>(
                (x, y) => string.Format("{0} {1}", x, y));
            var listener = evt.Snapshot(behavior, snapshotFunction).Listen(results.Add);

            evt.Send(100L);
            behavior.Send(2);
            evt.Send(200L);
            behavior.Send(9);
            behavior.Send(1);
            evt.Send(300L);
            listener.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("100 0", "200 2", "300 1"), results);
        }

        [Test]
        public void TestValues()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var listener = behavior.Value().Listen(x => { results.Add(x); });
            behavior.Send(2);
            behavior.Send(7);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestConstantBehavior()
        {
            var behavior = new Behavior<Int32>(12);
            var results = new List<Int32>();
            var listener = behavior.Value().Listen(x => { results.Add(x); });
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(12), results);
        }

        [Test]
        public void TestValuesThenMap()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var map = new Function<Int32, Int32>((x) => { return x + 100; });
            var l = behavior.Value().Map(map).Listen(x => { results.Add(x); });
            behavior.Send(2);
            behavior.Send(7);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(109, 102, 107), results);
        }

        [Test]
        public void TestValuesTwiceThenMap()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var map = new Function<Int32, Int32>((x) => { return x + 100; });
            var listener = DoubleUp(behavior.Value()).Map(map).Listen(x => { results.Add(x); });
            behavior.Send(2);
            behavior.Send(7);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(109, 109, 102, 102, 107, 107), results);
        }


        [Test]
        public void TestValuesThenCoalesce()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var coaleseFunction = new BinaryFunction<Int32, Int32, Int32>((fst, snd) => snd);
            var listener = behavior.Value().Coalesce(coaleseFunction).Listen(x => { results.Add(x); });
            behavior.Send(2);
            behavior.Send(7);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 2, 7), results);
        }

        
        [Test]
	    public void TestValuesTwiceThenCoalesce() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var coaleseFunction = new BinaryFunction<Int32, Int32, Int32>((fst, snd) => { return fst + snd; });
            var listener = DoubleUp(behavior.Value()).Coalesce(coaleseFunction).Listen(x => { results.Add(x); });
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(18, 4, 14), results);
	    }
        
        [Test]
	    public void TestValuesThenSnapshot() 
        {
		    var behaviorInt32 = new BehaviorSink<Int32>(9);
		    var behaviorChar = new BehaviorSink<char>('a');
		    var results = new List<char>();
            var listener = behaviorInt32.Value().Snapshot(behaviorChar).Listen(x => { results.Add(x); });
		    behaviorChar.Send('b');
		    behaviorInt32.Send(2);
		    behaviorChar.Send('c');
		    behaviorInt32.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('a', 'b', 'c'), results);
	    }
        
        [Test]
	    public void TestValuesTwiceThenSnapshot() 
        {
		    var behaviorInt32 = new BehaviorSink<Int32>(9);
		    var behaviorChar = new BehaviorSink<char>('a');
		    var results = new List<char>();
            var listener = DoubleUp(behaviorInt32.Value()).Snapshot(behaviorChar).Listen(x => { results.Add(x); });
		    behaviorChar.Send('b');
		    behaviorInt32.Send(2);
		    behaviorChar.Send('c');
		    behaviorInt32.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('a', 'a', 'b', 'b', 'c', 'c'), results);
	    }

        
        [Test]
	    public void TestValuesThenMerge() 
        {
		    var behavior1 = new BehaviorSink<Int32>(9);
		    var behavior2 = new BehaviorSink<Int32>(2);
		    var results = new List<Int32>();
            var combiningFunction = new BinaryFunction<Int32, Int32, Int32>((x, y) => x+y);
            var listener = Event<Int32>.MergeWith(combiningFunction, behavior1.Value(), behavior2.Value()).Listen(x => { results.Add(x); });
		    behavior1.Send(1);
		    behavior2.Send(4);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(11, 1, 4), results);
	    }
        
        [Test]
	    public void TestValuesThenFilter() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var predicate = new Function<int, bool>(a => true);
            var listener = behavior.Value().Filter(predicate).Listen(x => { results.Add(x); });
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 2, 7), results);
	    }
        
        [Test]
	    public void TestValuesTwiceThenFilter() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var predicate = new Function<Int32, bool>(a => true);
            var listener = DoubleUp(behavior.Value()).Filter(predicate).Listen(x => { results.Add(x); });
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 9, 2, 2, 7, 7), results);
	    }
        
        [Test]
	    public void TestValuesThenOnce() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var listener = behavior.Value().Once().Listen(x => { results.Add(x); });
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9), results);
	    }
        
        [Test]
	    public void TestValuesTwiceThenOnce() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var listener = DoubleUp(behavior.Value()).Once().Listen(x => { results.Add(x); });
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9), results);
	    }

        [Test]
	    public void TestValuesLateListen() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
		    var value = behavior.Value();
		    behavior.Send(8);
		    var listener = value.Listen(x => { results.Add(x); });
		    behavior.Send(2);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(8, 2), results);
	    }
	
        [Test]
	    public void TestMapB() 
        {
		    var behavior = new BehaviorSink<Int32>(6);
		    var results = new List<String>();
		    var listener = behavior.Map(x => x.ToString()).Value().Listen(x => { results.Add(x); });
		    behavior.Send(8);
		    listener.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("6", "8"), results);
	    }
	
        [Test]
	    public void TestMapBLateListen() 
        {
		    var behavior = new BehaviorSink<Int32>(6);
		    var results = new List<String>();
		    var map = behavior.Map(x => x.ToString());
		    behavior.Send(2);
		    var listener = map.Value().Listen(x => { results.Add(x); });
		    behavior.Send(8);
		    listener.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("2", "8"), results);
	    }
	
        [Test]
	    public void TestTransaction() 
        {
            var calledBack = new bool[1];
            Transaction.Run((trans) => { trans.Prioritized(Node.Null, (trans2) => { calledBack[0] = true; }); });
           Assert.AreEqual(true, calledBack[0]);
	    }

        [Test]
	    public void TestApply() 
        {
		    var bf = new BehaviorSink<IFunction<Int64, String>>(new Function<Int64, String>((b) => "1 "+b));
		    var ba = new BehaviorSink<Int64>(5L);
		    var results = new List<String>();
		    var listener = Behavior<Int64>.Apply<string>(bf,ba)
                .Value().Listen(x => { results.Add(x); });
		    bf.Send(new Function<Int64, String>((b) => "12 "+b));
		    ba.Send(6L);
            listener.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
	    }

        [Test]
	    public void TestLift() 
        {
		    var behavior1 = new BehaviorSink<Int32>(1);
		    var behavior2 = new BehaviorSink<Int64>(5L);
		    var results = new List<String>();
		    var listener = Behavior<Int32>.Lift((x, y) => x + " " + y, behavior1, behavior2)
                .Value().Listen((String x) => { results.Add(x); });
		    behavior1.Send(12);
		    behavior2.Send(6L);
            listener.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
	    }
	
        [Test]
	    public void TestLiftGlitch() 
        {
		    var behavior = new BehaviorSink<Int32>(1);
		    var behavior1 = behavior.Map(x => x * 3);
		    var behavior2 = behavior.Map(x => x * 5);
		    var behavior3 = Behavior<Int32>.Lift((x, y) => x + " " + y, behavior1, behavior2);
		    var results = new List<String>();
		    var listener = behavior3.Value().Listen((x) => { results.Add(x); });
		    behavior.Send(2);
		    listener.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("3 5", "6 10"), results);
	    }

        [Test]
	    public void TestHoldIsDelayed() 
        {
	        var e = new EventSink<Int32>();
	        var h = e.Hold(0);
	        var pair = e.Snapshot(h, (a, b) => a + " " + b);
		    var o = new List<String>();
		    var l = pair.Listen((String x) => { o.Add(x); });
		    e.Send(2);
		    e.Send(3);
		    l.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("2 0", "3 2"), o);
	    }

        [Test]
	    public void TestSwitchB()
	    {
	        var esb = new EventSink<SB>();
	        // Split each field o of SB so we can update multiple behaviours in a
	        // single transaction.
	        var ba = esb.Map(s => s.a).FilterNotNull().Hold('A');
	        var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
	        var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
	        var bo = Behavior<char?>.SwitchB(bsw);
		    var o = new List<char>();
	        var l = bo.Value().Listen(c => { o.Add(c.Value); });
	        esb.Send(new SB('B','b',null));
	        esb.Send(new SB('C','c',bb));
	        esb.Send(new SB('D','d',null));
	        esb.Send(new SB('E','e',ba));
	        esb.Send(new SB('F','f',null));
	        esb.Send(new SB(null,null,bb));
	        esb.Send(new SB(null,null,ba));
	        esb.Send(new SB('G','g',bb));
	        esb.Send(new SB('H','h',ba));
	        esb.Send(new SB('I','i',ba));
	        l.Unlisten();
	        AssertArraysEqual(Arrays<char>.AsList('A','B','c','d','E','F','f','F','g','H','I'), o);
	    }

        [Test]
        public void TestSwitchE()
        {
            var ese = new EventSink<SE>();
            var ea = ese.Map(s => s.a).FilterNotNull();
            var eb = ese.Map(s => s.b).FilterNotNull();
            var bsw = ese.Map(s => s.sw).FilterNotNull().Hold(ea);
            var o = new List<char>();
            var eo = Behavior<char>.SwitchE(bsw);
	        var l = eo.Listen(c => { o.Add(c); });
	        ese.Send(new SE('A','a',null));
	        ese.Send(new SE('B','b',null));
	        ese.Send(new SE('C','c',eb));
	        ese.Send(new SE('D','d',null));
	        ese.Send(new SE('E','e',ea));
	        ese.Send(new SE('F','f',null));
	        ese.Send(new SE('G','g',eb));
	        ese.Send(new SE('H','h',ea));
	        ese.Send(new SE('I','i',ea));
	        l.Unlisten();
	        AssertArraysEqual(Arrays<char>.AsList('A','B','C','d','e','F','G','h','I'), o);
        }

        [Test]
        public void TestLoopBehavior()
        {
            var ea = new EventSink<Int32>();
            var sum = new BehaviorLoop<Int32>();
            var sum_out = ea.Snapshot(sum, (x, y) => x+y).Hold(0);
            sum.Loop(sum_out);
            var o = new List<Int32>();
            var l = sum_out.Value().Listen(x => { o.Add(x); });
            ea.Send(2);
            ea.Send(3);
            ea.Send(1);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(0,2,5,6), o);
            Assert.AreEqual((int)6, (int)sum.Sample());
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventSink<Int32>();
            var o = new List<Int32>();
            var sum = ea.Hold(100).Collect(0,
                //(a,s) -> new Tuple2(a+s, a+s)
                new BinaryFunction<Int32, Int32, Tuple2<Int32, Int32>>((a, s) => 
                {
                    return new Tuple2<Int32, Int32>(a + s, a + s);
                })
            );
            var l = sum.Value().Listen((x) => { o.Add(x); });
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(100,105,112,113,115,118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventSink<Int32>();
            var o = new List<Int32>();
            var sum = ea.Accumulate(100, (a,s)=>a+s);
            var l = sum.Value().Listen((x) => { o.Add(x); });
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(100,105,112,113,115,118), o);
        }
        
        /// <summary>
        /// This is used for tests where value() produces a single initial value on listen,
        /// and then we double that up by causing that single initial event to be repeated.
        /// This needs testing separately, because the code must be done carefully to achieve
        /// this.
        /// </summary>
        private static Event<Int32> DoubleUp(Event<Int32> ev)
        {
            return Event<Int32>.Merge(ev, ev);
        }

        

        class SE
	    {
	        public SE(char a, char b, Event<char> sw)
	        {
	            this.a = a;
	            this.b = b;
	            this.sw = sw;
	        }
	        public char a;
	        public char b;
            public Event<char> sw;
	    }

        class SB
	    {
            public SB(char? a, char? b, Behavior<char?> sw)
	        {
	            this.a = a;
	            this.b = b;
	            this.sw = sw;
	        }
            public char? a;
            public char? b;
            public Behavior<char?> sw;
	    }
    }
}