using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissionPlanner_console
{
   public struct TestData
   {
      public String cmndSet;
      public String navOutput;

      public TestData(String cmdSetInput, String desiredOutput)
      {
         cmndSet = cmdSetInput;
         navOutput = desiredOutput;
      }
   };

   class TestRoverNav
   {
      public void PerformAllTests()
      {
         TestDataValidity();
         TestZoneBounds();
         TestRoverCommands();
         System.Console.ReadKey();
      }

      public bool TestDataValidity()
      {
         TestData[] testDataList = new TestData[9]
         {
            new TestData("[0, 0] [8, 10] [1, 2, E] [MMLMRMMRRMML]", "[3, 3, S] [0x00000001, 0x00000000]"),
            new TestData("[0, E] [8, 10] [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[S, 0] [8, 10] [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[0, 0] [S, 10] [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[0, 0] [8, F]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[0, 0] [8, 10] [S, 1, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[0, 0] [8, 10] [1, W, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[0, 0] [8, 10] [1, 2, 3] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
            new TestData("[0, 0] [8, 10] [1, 2, 3] [MMKMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000001]"),
         };

         System.Console.WriteLine("Testing input data...");
         return PerformTest(ref testDataList);       
      }

      public bool TestZoneBounds()
      {
         TestData[] testDataList = new TestData[10]
         {
            new TestData("[0, 0]   [8, 10]  [1, 2, E] [MMLMRMMRRMML]", "[3, 3, S] [0x00000001, 0x00000000]"),
            new TestData("[-1, -1] [8, 10]  [1, 2, E] [MMLMRMMRRMML]", "[3, 3, S] [0x00000001, 0x00000000]"),
            new TestData("[8, 0]   [8, 10]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000006]"),
            new TestData("[0, 10]  [8, 10]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000007]"),
            new TestData("[9, 0]   [8, 10]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000004]"),
            new TestData("[0, 11]  [8, 10]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000005]"),
            new TestData("[0, 0]   [0, 10]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000006]"),
            new TestData("[0, 0]   [8, 0]   [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000007]"),
            new TestData("[0, 0]   [-1, 10] [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000004]"),
            new TestData("[0, 0]   [8, -1]  [1, 2, E] [MMLMRMMRRMML]", "[0, 0, N] [0x00000001, 0x00000005]"),
         };

         System.Console.WriteLine("\nTesting zone bounds...");
         return PerformTest(ref testDataList); 
      }

      public bool TestRoverCommands()
      {
         TestData[] testDataList = new TestData[7]
         {
            new TestData("[0, 0]  [8, 10] [1, 2, E] [MMLMRMMRRMML]", "[3, 3, S] [0x00000001, 0x00000000]"),
            new TestData("[0, 0]  [8, 10] [1, 2, E] [LLMMM]",        "[0, 2, W] [0x00000001, 0x00000006]"),
            new TestData("[-1, 0] [8, 10] [1, 2, E] [LLMMM]",        "[-1, 2, W] [0x00000001, 0x00000006]"),
            new TestData("[-1, 0] [8, 10] [1, 2, E] [MMMMMMMM]",     "[8, 2, E] [0x00000001, 0x00000006]"),
            new TestData("[0, -1] [8, 10] [1, 2, E] [RMMMM]",        "[1, -1, S] [0x00000001, 0x00000007]"),
            new TestData("[0, -1] [8, 10] [1, 2, E] [LMMMMMMMMM]",   "[1, 10, N] [0x00000001, 0x00000007]"),
            new TestData("[-4, -4] [-1, -1] [-1, -1, W] [MMMLMMM]", "[-4, -4, S] [0x00000001, 0x00000000]"),
         };

         System.Console.WriteLine("\nTesting rover commands...");
         return PerformTest(ref testDataList);       
      }

      private bool PerformTest(ref TestData[] testDataList)
      {
         bool testPassed = true;
         RoverNav roverNav = new RoverNav(false);
         foreach (TestData testData in testDataList)
         {
            CmdSet cmndSet = new CmdSet();
            cmndSet.zoneMin.x = 0;
            cmndSet.zoneMin.y = 0;
            cmndSet.zoneMax.x = 0;
            cmndSet.zoneMax.y = 0;
            cmndSet.startPos.coords.x = 0;
            cmndSet.startPos.coords.y = 0;
            cmndSet.startPos.cardinalPt = CardinalPoint.North;
            cmndSet.commands = "";

            RNResult rnRes = roverNav.ParseCommandSet(testData.cmndSet, ref cmndSet);
            if (rnRes.severity == Severity.Success)
               rnRes = roverNav.ValidateCommandSet(cmndSet);

            Position curPos = roverNav.CurrentPosition;
            String outputData = String.Format("{0} {1}", roverNav.FormatPosition(curPos), rnRes.FormatResult());

            String comparisonStr = "Success";
            if (outputData != testData.navOutput)
            {
               comparisonStr = "Fail";
               testPassed = false;
            }
            
            System.Console.WriteLine("..." + testData.cmndSet + "..." + comparisonStr);
         }

         return testPassed;
      }
   }
}
