using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissionPlanner_console
{
   class Program
   {
      /* Function: Main
       * Purpose : 
       * Notes   :
       */
      static void Main(string[] args)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);
         bool displayData = true;

         if (args.Length == 1)
         {
            if (args[0] == "Test")
            {
               TestRoverNav testRoverNav = new TestRoverNav();
               testRoverNav.PerformAllTests();
               return;
            }
         }

         if (args.Length != 2)
         {
            System.Console.WriteLine("Syntax:\n\n");
            System.Console.WriteLine("RoverNavSystem [intput file] [output file]\n\n");
            System.Console.WriteLine("   [input file] is the input file with the following format:\n");
            System.Console.WriteLine("      [xmin, ymin] [xmax, ymax] [xstart, ystart, hstart] [commands]");
            System.Console.WriteLine("         where");
            System.Console.WriteLine("            [xmin, ymin] is the minimum cartesian coordinate of the zone's boundary.");
            System.Console.WriteLine("            [xmax, ymax] is the maximum cartesian coordinate of the zone's boundary.");
            System.Console.WriteLine("            [xstart, ystart, hstart] is the starting position and heading of the rover.");
            System.Console.WriteLine("            [commands] is a list of commands, directing the rover where to go.\n");
            System.Console.WriteLine("   [output file] is the output file with the following format:\n");
            System.Console.WriteLine("      [xpos, ypos, hpos] [moduleId, resultcode]");
            System.Console.WriteLine("         where");
            System.Console.WriteLine("            [xpos, ypos, hpos] is the resulting position and heading of the rover.");
            System.Console.WriteLine("            [moduleId, resultcode] is the module ID and resultcode.");
            System.Console.ReadKey();
            return;
         }

         String inputFile = args[0];
         String outputFile = args[1];

         // Verify that the file exists.
         if (!System.IO.File.Exists(inputFile))
         {
            System.Console.WriteLine("The file, {0}, does not exists.\n", inputFile);
            return;
         }

         // Read the input from a file.
         String inputText = "";
         try
         {
            inputText = System.IO.File.ReadAllText(inputFile);
         }
         catch
         {
            System.Console.WriteLine("Unable to read the file, {0}.\n", inputFile);
            return;
         }

         /* Initialize the command set structure */
         CmdSet cmndSet = new CmdSet();
         cmndSet.zoneMin.x = 0;
         cmndSet.zoneMin.y = 0;
         cmndSet.zoneMax.x = 0;
         cmndSet.zoneMax.y = 0;
         cmndSet.startPos.coords.x = 0;
         cmndSet.startPos.coords.y = 0;
         cmndSet.startPos.cardinalPt = CardinalPoint.North;
         cmndSet.commands = "";

         RoverNav roverNav = new RoverNav(displayData);         
         rnRes = roverNav.ParseCommandSet(inputText, ref cmndSet);
         if (rnRes.severity == Severity.Success)
            rnRes = roverNav.ValidateCommandSet(cmndSet);

         if (rnRes.severity == Severity.Success)
         {
            System.Console.WriteLine("The command set in file, {0}, executed successfully and is ready for transmission.\n", inputFile);
         }
         else
         {
            System.Console.WriteLine("Unable to execute the command set in file, {0}.\n", inputFile);
            DisplayError(rnRes);            
         }

         // Get the current position even if the command set resulted in an error.
         Position curPos = roverNav.CurrentPosition;            
         String dataToSend = String.Format("{0} {1}", roverNav.FormatPosition(curPos), rnRes.FormatResult());
         System.IO.File.WriteAllText(outputFile, dataToSend);

         if (displayData)
         {
            System.Console.WriteLine(dataToSend);
         }         
      }

      /* Function: DisplayError
       * Purpose : 
       * Notes   :
       */
      static void DisplayError(RNResult rnRes)
      {
         if (rnRes.module == ModuleId.RoverNav)
         {
            switch (rnRes.error)
            {
               case (Int32)RoverNavResult.ErrInvalidCmdChar:
                  System.Console.WriteLine("One or more of the command set parameters contain invalid input data.\n");
                  break;
               case (Int32)RoverNavResult.ErrEmptyCmdsetParam:
                  System.Console.WriteLine("One or more of the command set parameters are empty.\n");
                  break;
               case (Int32)RoverNavResult.ErrInvalidNoCmdsetParams:
                  System.Console.WriteLine("There are no valid command set parameters.\n");
                  break;
               case (Int32)RoverNavResult.ErrZoneBoundsX:
                  System.Console.WriteLine("The x-coordinate of the zone bounds are invalid.\n");
                  break;
               case (Int32)RoverNavResult.ErrZoneBoundsY:
                  System.Console.WriteLine("The y-coordinate of the zone bounds are invalid.\n");
                  break;
               case (Int32)RoverNavResult.ErrRoverBoundsX:
                  System.Console.WriteLine("The x-coordinate of the rover position is beyond the zone bound.\n");
                  break;
               case (Int32)RoverNavResult.ErrRoverBoundsY:
                  System.Console.WriteLine("The y-coordinate of the rover position is beyond than the zone bound.\n");
                  break;
            }
         }
      }
   }
}
