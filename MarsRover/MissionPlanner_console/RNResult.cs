using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MissionPlanner_console
{
   public enum Severity : byte
   { 
      Success = 0,
      Error = 1
   };

   public enum ModuleId : int
   {
      OS,
      RoverNav,
      MissionPlanner
   };

   public struct RNResult
   {
      public Severity severity;
      public ModuleId module;
      public int error;

      public RNResult(Severity resSeverity, ModuleId resModuleId, int resError)
      {
         severity = resSeverity;
         module = resModuleId;
         error = resError;
      }

      public String FormatResult()
      {
         String resultStr = String.Format("[0x{0}, 0x{1}]", module.ToString("X"), error.ToString("X8"));
         return resultStr;
      }
   }
}
