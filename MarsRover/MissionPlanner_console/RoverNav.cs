using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissionPlanner_console
{
   public enum RoverNavResult : int
   { 
      Success = 0,
      ErrInvalidCmdChar,
      ErrEmptyCmdsetParam,
      ErrInvalidNoCmdsetParams,
      ErrZoneBoundsX,
      ErrZoneBoundsY,
      ErrRoverBoundsX,
      ErrRoverBoundsY,
   };

   public enum CardinalPoint
   {
      North = 0,
      East  = 1,
      South = 2,
      West  = 3
   };

   /* Unit is blocks. Assume that maximum block coordinate will not exceed the size of an int */
   public struct Coords
   {
      public int x, y;
      public Coords(int x1, int y1)
      {
         x = x1;
         y = y1;
      }
   };

   public struct Position
   {
      public Coords coords;
      public CardinalPoint cardinalPt;

      public Position(Coords c1, CardinalPoint cPt1)
      {
         coords = c1;
         cardinalPt = cPt1;
      }
   };

   public struct CmdSet
   {
      public Coords zoneMin;
      public Coords zoneMax;
      public Position startPos;
      public string commands;
   };

   class RoverNav
   {
      public const uint noCmndsetParams = 4;

      /* Function: Constructor
       * Purpose : 
       * Notes   :
       */ 
      public RoverNav(bool displayData)
      {
         Initialise();
         display = displayData;         
      }

      /* Function: Destructor
       * Purpose : 
       * Notes   :
       */ 
      ~RoverNav()
      {
      }

      /* Function: CurrentPosition
       * Purpose : Property to obtain the rover's current position.
       * Notes   :
       */
      public Position CurrentPosition
      {
         get
         {
            return curPos;
         }
      }

      /* Function: ParseCommandSet
       * Purpose : 
       * Notes   :
       */ 
      public RNResult ParseCommandSet(String cmndsetStr, ref CmdSet cmdset)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);
         String leftDelimeter = "[";
         String rightDelimeter = "]";

         // Initialise the member variables
         Initialise();

         // Remove redundant characters from the string 
         String resultStr = "";
         for (int i=0; i<cmndsetStr.Length; i++)
         {
            if ((cmndsetStr[i] != ' ') && (cmndsetStr[i] != '\r') && (cmndsetStr[i] != '\n'))
            {
               resultStr += cmndsetStr[i];
            }
         }

         // Extract the parameters (enclosed with []) of the command set
         ArrayList cmdsetParams = new ArrayList();
         rnRes = GetEnclosedParams(resultStr, leftDelimeter, rightDelimeter, cmdsetParams);
         if (rnRes.severity != Severity.Success)
            return rnRes;

         // Validate the parameter data
         rnRes = VerifyCommandSetData(cmdsetParams, ref cmdset);
         if (rnRes.severity != Severity.Success)
            return rnRes;

         return rnRes;
      }

      /* Function: ValidateCommandSet
       * Purpose : 
       * Notes   :
       */ 
      public RNResult ValidateCommandSet(CmdSet cmdset)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);

         rnRes = VerifyZoneBounds(cmdset.zoneMin, cmdset.zoneMax);
         if (rnRes.severity != Severity.Success)
            return rnRes;
         zoneMin = cmdset.zoneMin;
         zoneMax = cmdset.zoneMax;

         rnRes = VerifyCoords(cmdset.startPos.coords);
         if (rnRes.severity != Severity.Success)
            return rnRes;
         curPos.coords = cmdset.startPos.coords;
         curPos.cardinalPt = cmdset.startPos.cardinalPt;

         rnRes = ExecuteCommands(cmdset.commands);

         return rnRes;
      }

      /* Function: FormatPosition
       * Purpose : Format the position for printing.
       * Notes   :
       */
      public string FormatPosition(Position pos)
      {
         string posStr = String.Format("[{0}, {1}, {2}]", pos.coords.x, pos.coords.y, CardinalPointToChar(pos.cardinalPt));
         return posStr;
      }

      // Private member variables
      private Position curPos;
      private Coords zoneMin;
      private Coords zoneMax;
      private bool display;

      // Private member functions 

      /* Function: VerifyCommandSetData
       * Purpose : 
       * Notes   :
       */
      private RNResult VerifyCommandSetData(ArrayList cmdsetParams, ref CmdSet cmdset)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);
         String paramDelimeter = ",";
         String sign = "-";
         String numericChars = "0123456789";
         String orientationChars = "NESW";
         String cmndChars = "RLM";

         // Verify the first parameter, the minimum zone coordinate
         ArrayList subParams = new ArrayList();
         ArrayList validChars = new ArrayList();
         validChars.Add(sign + numericChars);
         validChars.Add(sign + numericChars);         
         rnRes = GetParams(cmdsetParams[0].ToString(), paramDelimeter, validChars, subParams);
         if (rnRes.severity == Severity.Success)
         {
            cmdset.zoneMin.x = System.Convert.ToInt32(subParams[0]);
            cmdset.zoneMin.y = System.Convert.ToInt32(subParams[1]);
         }              

         // Verify the second parameter, the maximum zone coordinate
         subParams.Clear();
         validChars.Clear();                 
         if (rnRes.severity == Severity.Success)
         {
            validChars.Add(sign + numericChars);
            validChars.Add(sign + numericChars);
            rnRes = GetParams(cmdsetParams[1].ToString(), paramDelimeter, validChars, subParams);
            if (rnRes.severity == Severity.Success)
            {
               cmdset.zoneMax.x = System.Convert.ToInt32(subParams[0]);
               cmdset.zoneMax.y = System.Convert.ToInt32(subParams[1]);
            }
         }

         // Verify the third parameter, the rover's starting position
         subParams.Clear();
         validChars.Clear(); 
         if (rnRes.severity == Severity.Success)
         {
            validChars.Add(sign + numericChars);
            validChars.Add(sign + numericChars);
            validChars.Add(orientationChars);
            rnRes = GetParams(cmdsetParams[2].ToString(), paramDelimeter, validChars, subParams);
            
            if (rnRes.severity == Severity.Success)
            {
               cmdset.startPos.coords.x = System.Convert.ToInt32(subParams[0]);
               cmdset.startPos.coords.y = System.Convert.ToInt32(subParams[1]);
               String cardinalPtChar = subParams[2].ToString();
               cmdset.startPos.cardinalPt = CharToCardinalPoint(cardinalPtChar[0]);
            }
         }

         // Verify the fourth parameter, the list of commands
         subParams.Clear();
         validChars.Clear();
         if (rnRes.severity == Severity.Success)
         {
            validChars.Add(cmndChars);
            rnRes = GetParams(cmdsetParams[3].ToString(), paramDelimeter, validChars, subParams);
            if (rnRes.severity == Severity.Success)
            {
               cmdset.commands = subParams[0].ToString();
            }
         }

         return rnRes;
      }

      /* Function: VerifyZoneBounds
       * Purpose : Verify the validity of the zone bounds.
       * Notes   :
       */
      private RNResult VerifyZoneBounds(Coords zoneMinCoord, Coords zoneMaxCoord)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);

         if (zoneMaxCoord.x < zoneMinCoord.x)
            rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrZoneBoundsX);

         if (rnRes.severity == Severity.Success)
         { 
            if (zoneMaxCoord.y < zoneMinCoord.y)
               rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrZoneBoundsY);
         }

         return rnRes;
      }

      /* Function: VerifyCoords
       * Purpose : Verify that the rover coordinates falls within the zone bounds.
       * Notes   :
       */
      private RNResult VerifyCoords(Coords newRoverCoords)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);

         if ((newRoverCoords.x < zoneMin.x) || (newRoverCoords.x > zoneMax.x))
         { 
            rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrRoverBoundsX);
         }
         
         if ((newRoverCoords.y < zoneMin.y) || (newRoverCoords.y > zoneMax.y))
         { 
            rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrRoverBoundsY);
         }
    
         return rnRes;
      }

      /* Function: ExecuteCommands
       * Purpose : 
       * Notes   :
       */
      private RNResult ExecuteCommands(String commands)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);

         /* Process each command */
         Display(curPos);

         char[] cmnds = commands.ToCharArray();
         for (uint i = 0; i < commands.Length; i++)
         {
            switch (cmnds[i])
            {
               case 'L':
                  if (curPos.cardinalPt == CardinalPoint.North)
                     curPos.cardinalPt = CardinalPoint.West;
                  else
                     curPos.cardinalPt = (CardinalPoint)(curPos.cardinalPt - 1);
                  break;

               case 'R':
                  if (curPos.cardinalPt == CardinalPoint.West)
                     curPos.cardinalPt = CardinalPoint.North;
                  else
                     curPos.cardinalPt = (CardinalPoint)(curPos.cardinalPt + 1);
                  break;

               case 'M':
                  /* Before moving, first test if it is a valid move */
                  Coords newCoords;
                  newCoords.x = curPos.coords.x;
                  newCoords.y = curPos.coords.y;

                  if (curPos.cardinalPt == CardinalPoint.North)
                     newCoords.y = curPos.coords.y + 1;
                  if (curPos.cardinalPt == CardinalPoint.South)
                     newCoords.y = curPos.coords.y - 1;
                  if (curPos.cardinalPt == CardinalPoint.East)
                     newCoords.x = curPos.coords.x + 1;
                  if (curPos.cardinalPt == CardinalPoint.West)
                     newCoords.x = curPos.coords.x - 1;

                  rnRes = VerifyCoords(newCoords);
                  if (rnRes.severity == Severity.Success)
                  {
                     curPos.coords.x = newCoords.x;
                     curPos.coords.y = newCoords.y;
                  }     

                  break;
               default:
                  // No default as input has been checked.
                  break;
            }

            // Stop processing the commands if there is an error.
            Display(curPos);
            if (rnRes.severity != Severity.Success)
               break;
         }         

         return rnRes;
      }

      /* Function: GetEnclosedParams
       * Purpose : 
       * Notes   :
       */
      private RNResult GetEnclosedParams(String paramStr, 
                                         String leftDelimeter,
                                         String rightDelimeter,
                                         ArrayList cmdsetParams)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);

         int startIndex = 0;
         int endIndex = 0;
         int paramstrLen = paramStr.Length;
         while ((startIndex >= 0) && (startIndex < paramstrLen))
         {
            try
            {
               startIndex = paramStr.IndexOf(leftDelimeter, startIndex);
               endIndex = paramStr.IndexOf(rightDelimeter, startIndex);

               if ((startIndex != -1) && (endIndex != -1) && (endIndex > startIndex) && (endIndex < paramstrLen))
               {
                  int paramLen = endIndex - startIndex - 1;
                  if (paramLen > 0)
                  {
                     String parameter = paramStr.Substring(startIndex + 1, paramLen);
                     if (parameter.Length > 0)
                        cmdsetParams.Add(parameter);

                     if (endIndex < (paramstrLen - 1))
                     {
                        endIndex++;
                        startIndex = endIndex;
                     }
                     else
                        startIndex = endIndex = -1;
                  }
                  else
                     rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrEmptyCmdsetParam);
               }
            }
            catch
            {
            }

            if (rnRes.severity != Severity.Success)
               startIndex = endIndex = -1;
         }

         if (cmdsetParams.Count != noCmndsetParams)
            rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrInvalidNoCmdsetParams);

         return rnRes;
      }

      /* Function: GetParams
       * Purpose : 
       * Notes   :
       */
      private RNResult GetParams(String paramStr, 
                                 String paramDelimeter,
                                 ArrayList validChars, 
                                 ArrayList paramsList)
      {
         RNResult rnRes = new RNResult(Severity.Success, ModuleId.RoverNav, (int)RoverNavResult.Success);

         int startIndex = 0;
         int endIndex = 0;
         int paramstrLen = paramStr.Length;
         while ((startIndex >= 0) && (endIndex >= 0) && (startIndex < paramstrLen))
         {
            int paramLen = 0;

            endIndex = paramStr.IndexOf(paramDelimeter, startIndex);
            if (endIndex == -1)
               paramLen = paramstrLen - startIndex;
            else  
               paramLen = endIndex - startIndex;

            if (paramLen > 0)
            {
               String parameter = paramStr.Substring(startIndex, paramLen);
               if (parameter.Length > 0)
                 paramsList.Add(parameter);

               startIndex = endIndex + 1; 
            }
         }

         // Does the parameters contain only valid characters         
         int expectedNoParams = validChars.Count;
         int noParams = paramsList.Count;         
         if (expectedNoParams == noParams) 
         {
            for (int i=0; i<noParams; i++)
            {
               String param = paramsList[i].ToString();
               String validCharsForParam = validChars[i].ToString();
               
               int j=0;
               for (j=0; j<param.Length; j++)
               {
                  if (!validCharsForParam.Contains(param[j]))
                     break;               
               }
           
               if (j < param.Length)
               {
                  rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrInvalidCmdChar); 
                  break;
               }
            }
         }
         else
            rnRes = new RNResult(Severity.Error, ModuleId.RoverNav, (int)RoverNavResult.ErrInvalidNoCmdsetParams);
         
         return rnRes;
      }


      /* Function: CardinalPointToChar
       * Purpose : 
       * Notes   :
       */
      private char CardinalPointToChar(CardinalPoint cpt)
      {
         switch (cpt)
         {
            case CardinalPoint.North:
               return 'N';
            case CardinalPoint.East:
               return 'E';
            case CardinalPoint.South:
               return 'S';
            case CardinalPoint.West:
               return 'W';
         }
         /* Not really a default value, but we should not get here */
         return 'N';
      }

      /* Function: CharToCardinalPoint
       * Purpose : 
       * Notes   :
       */
      private CardinalPoint CharToCardinalPoint(char orientation)
      {
         CardinalPoint cpt = CardinalPoint.North;
         if (orientation == 'E')
            cpt = CardinalPoint.East;
         if (orientation == 'S')
            cpt = CardinalPoint.South;
         if (orientation == 'W')
            cpt = CardinalPoint.West;
         return cpt;
      }
      
      /* Function: Display
       * Purpose : 
       * Notes   :
       */
      private void Display(Position pos)
      {
         if (display)
         {
            string posStr = FormatPosition(pos);
            System.Console.WriteLine(posStr);
         }
      }

      /* Function: Initialise
       * Purpose : 
       * Notes   :
       */
      private void Initialise()
      {
         curPos.coords.x = 0;
         curPos.coords.y = 0;
         curPos.cardinalPt = CardinalPoint.North;
         zoneMin.x = 0;
         zoneMin.y = 0;
         zoneMax.x = 0;
         zoneMax.y = 0;
      }
   }
}
