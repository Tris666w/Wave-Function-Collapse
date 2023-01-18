using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WfcReturn
{
   public enum WfcReturnState
   {
      Succes,
      Warning,
      Error
   }
   public WfcReturnState returnState;
   public string returnContext;
}
