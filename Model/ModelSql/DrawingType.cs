using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum DrawingType
    {
        None = 0,
        [Description("Piping and Instriment Diagram")]
        PipingAndInstrimentDiagram = 1,
        [Description("Cause and Effect")]
        CauseAndEffect = 2,
        [Description("Process Flow Diagram")]
        ProcessFlowDiagram = 3,
        [Description("Master Equipment List")]
        MasterEquipmentList = 4,
        [Description("Master Linet List")]
        MasterLinetList = 5,
        Calculation = 6,
        Report = 7,
    }
}
