using System;
using System.Collections.Generic;
using System.Text;

namespace PdxScriptPlusPlus.Script
{
    interface IKeyNode
    {
         String Key { get; set; }
    }

    interface IValueNode
    {
        String Value { get; set; }

		Boolean HasStringValue();
	}

}
