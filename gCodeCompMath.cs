using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CNC3
{
    public enum MathItemType_et
    {
        bracketOpen,
        bracketClose,
        doubleVar,
        constVar,
        oper_add,
        oper_sub,
        oper_mul,
        oper_div,
        oper_power,
        oper_mod,
        func_sin,
        func_cos,
        func_tan,
        func_asin,
        func_acos,
        func_atan,
        func_sqrt,
        oper_and,
        oper_or,
        oper_xor,
        oper_eq,
        oper_ne,
        oper_gt,
        oper_ge,
        oper_lt,
        oper_le,
        unknown
    }

    

    public class MathItem
    {
        public string valueStr { get; set; }
        public double value { get; set; }
        public int idx { get; set; }


        public MathItem()
        {

        }
        public MathItem(MathItemType_et newType)
        {
            type = newType;
        }
        public MathItem(MathItem item)
        {
            value = item.value;
            type = item.type;
            idx = item.idx;
            valueStr = item.valueStr;
        }

        public MathItemType_et type { get; set; }
        public int GetPriority()
        {
            switch (type)
            {
                case MathItemType_et.oper_power:
                    return 6;
                case MathItemType_et.oper_mul:
                case MathItemType_et.oper_div:
                case MathItemType_et.oper_mod:
                    return 5;
                case MathItemType_et.oper_add:
                case MathItemType_et.oper_sub:
                    return 4;
                case MathItemType_et.oper_eq:
                case MathItemType_et.oper_ne:
                case MathItemType_et.oper_gt:
                case MathItemType_et.oper_ge:
                case MathItemType_et.oper_lt:
                case MathItemType_et.oper_le:
                    return 3;
                case MathItemType_et.oper_and:
                case MathItemType_et.oper_or:
                case MathItemType_et.oper_xor:
                    return 2;
                case MathItemType_et.func_sin:
                case MathItemType_et.func_cos:
                case MathItemType_et.func_tan:
                case MathItemType_et.func_sqrt:
                case MathItemType_et.func_asin:
                case MathItemType_et.func_acos:
                case MathItemType_et.func_atan:
                    return 1;
                default:
                    return 0;
            }
        }

        public enum SyntaxGrp_et
        {
            grp_var,
            grp_bracketOpen,
            grp_bracketClose,
            grp_funcBracket,
            grp_funcTwoArg,
            grp_funcTwoArgStart,
            grp_unknown
        }

        public SyntaxGrp_et GetSyntaxGroup()
        {
            switch (type)
            {
                case MathItemType_et.constVar:
                case MathItemType_et.doubleVar:
                    return SyntaxGrp_et.grp_var;
                case MathItemType_et.bracketOpen:
                    return SyntaxGrp_et.grp_bracketOpen;
                case MathItemType_et.bracketClose:
                    return SyntaxGrp_et.grp_bracketClose;
                case MathItemType_et.func_sin:
                case MathItemType_et.func_cos:
                case MathItemType_et.func_tan:
                case MathItemType_et.func_asin:
                case MathItemType_et.func_acos:
                case MathItemType_et.func_atan:
                case MathItemType_et.oper_mod:
                case MathItemType_et.func_sqrt:
                    return SyntaxGrp_et.grp_funcBracket; /* need brackets */
                case MathItemType_et.oper_add:
                case MathItemType_et.oper_sub:
                    return SyntaxGrp_et.grp_funcTwoArgStart; /* may exists at start */
                default: /* */ 
                    return SyntaxGrp_et.grp_funcTwoArg;
            }
        }

    }

    public class gCodeCompMath
    {
        public delegate void CallbackErrorCallback(string errorMsg);
        public static event CallbackErrorCallback ErrorCallback;


        List<VariableArrayItem> variableArray = new List<VariableArrayItem>();
        public static double[] globalArray = new double[7000];
        public static List<VariableArrayItem> globalVariableArray = new List<VariableArrayItem>();

        public static void ClearStatic()
        {
            globalVariableArray.Clear();
        }



        public enum VariableType_et
        {
            VAR_NUM_GLOBAL,
            VAR_STR_GLOBAL,
            VAR_STR_LOCAL,
            VAR_UNN
        }


        public void Clear()
        {
            variableArray.Clear();
        }
        private double ReadVariable(int idx)
        {
            if(idx < 0)
            {
                /* TODO error */
                return double.NaN;
            }
            else if(idx < 10000)
            {
                if(idx < globalArray.Length)
                {
                    return globalArray[idx];
                }
                else
                {
                    /* TODO error */
                    return double.NaN;
                }
            }
            else if(idx < 1000000)
            {
                idx = idx - 10000;

                if ((globalVariableArray.Count > idx) && (idx >= 0))
                {
                    return globalVariableArray[idx].value;
                }
                else
                {
                    /* TODO error */
                    return double.NaN;
                }

            }
            else
            {
                idx = idx - 1000000;

                if ((variableArray.Count > idx) && (idx >= 0))
                {
                    return variableArray[idx].value;
                }
                else
                {
                    /* TODO error */
                    return double.NaN;
                }
            }
        }

        private int GetGlobalIdx(string idxStr)
        {
            int idx = -1;

            try
            {
                idx = Convert.ToUInt16(idxStr.Substring(1));
            }
            catch (FormatException)
            {
                /* TODO ERROR */
                ErrorCallback("Global variable invalid number");
                idx = -1;
            }
            catch (OverflowException)
            {
                idx =  -1;
            }
            return idx;
        }

        private int WriteGlobal(int idx, double val)
        {
            if ((idx < 5000) & (idx >= 0))
            {
                globalArray[idx] = val;
                return idx;
            } 
            else 
            { 
                return -1; 
            }
        }

        private int GetVariableIdx(string name)
        {
            int idx = -1;

            if (name.Length > 3 && name[1] == '<' && name[name.Length-1] == '>')
            {
                if (name[2] == '_')
                {
                    if(name.Length > 4)
                    {
                        idx = globalVariableArray.FindIndex(x => x.name == name);
                    }
                    if(idx != -1)
                    {
                        idx += 10000;
                    }
                }
                else
                {
                    if (name.Length > 3)
                    {
                        idx = variableArray.FindIndex(x => x.name == name);
                    }
                    if (idx != -1)
                    {
                        idx += 1000000;
                    }
                }
            }
            else
            {
                idx = GetGlobalIdx(name);
            }

            return idx;
        }

        public int WriteWariableByStr(string varName,  double val)
        {

            int idx = -1;


            if (varName.Length >= 2)
            {
                if (varName[1] == '<' && varName[varName.Length - 1] == '>')
                {
                    if (varName[2] == '_')
                    {
                        if (varName.Length > 4)
                        {
                            /* str global */
                            idx = globalVariableArray.FindIndex(x => x.name == varName);

                            if (idx == -1)
                            {
                                VariableArrayItem item2 = new VariableArrayItem();
                                item2.name = varName;
                                item2.value = 0;
                                globalVariableArray.Add(item2);
                                idx = globalVariableArray.FindIndex(x => x.name == varName);

                            }
                            else
                            {
                                globalVariableArray[idx].value = 0;
                            }

                            if (idx != -1)
                            {
                                idx += 10000;
                            }
                        }
                    }
                    else
                    {
                        if (varName.Length > 3)
                        {
                            /* str local */
                            idx = variableArray.FindIndex(x => x.name == varName);

                            if (idx == -1)
                            {
                                VariableArrayItem item2 = new VariableArrayItem();
                                item2.name = varName;
                                item2.value = 0;
                                variableArray.Add(item2);
                                idx = variableArray.FindIndex(x => x.name == varName);

                            }
                            else
                            {
                                variableArray[idx].value = 0;
                            }

                            if (idx != -1)
                            {
                                idx += 1000000;
                            }

                        }
                    }
                }
                else
                {
                    /* num global */
                    idx =  WriteGlobal(GetGlobalIdx(varName), val);
                }
            }

            return idx;
        }

        public int WriteWariableByIdx(int idx, double val)
        {
            if(idx<0)
            {
                return -1;
            }
            else if(idx < 10000)
            {
                /*  numeric global */
                return WriteGlobal(idx, val);
            }
            else if(idx <1000000)
            {
                /*str global */
                idx = idx - 10000;

                if ((idx >= 0) && (globalVariableArray.Count > idx))
                {
                    globalVariableArray[idx].value = val;
                    return 0;
                }
                else
                {
                    return -1;
                }

            }
            else
            {
                /*str local */
                idx = idx - 1000000;

                if ((idx >= 0) && (variableArray.Count > idx))
                {
                    variableArray[idx].value = val;
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }

        private double CalcFunction(double data, MathItemType_et type)
        {
            switch (type)
            {
                case MathItemType_et.func_sin:
                    return Math.Sin((Math.PI / 180) * data);

                case MathItemType_et.func_cos:
                    return Math.Cos((Math.PI / 180) * data);

                case MathItemType_et.func_tan:
                    return Math.Tan((Math.PI / 180) * data);

                case MathItemType_et.func_asin:
                    return Math.Asin( data) * 180 / Math.PI;

                case MathItemType_et.func_acos:
                    return Math.Acos( data) * 180 / Math.PI;

                case MathItemType_et.func_atan:
                    return Math.Atan( data) * 180 / Math.PI;

                case MathItemType_et.func_sqrt:
                    return Math.Sqrt(data);

                default:
                    ErrorCallback("MATH: unknown function");
                    return double.NaN;
            }
        }

        private double CalcOperator(double data1, double data2, MathItemType_et type)
        {
            switch (type)
            {
                case MathItemType_et.oper_add:
                    return data1 + data2;
                case MathItemType_et.oper_sub:
                    return data1 - data2;
                case MathItemType_et.oper_mul:
                    return data1 * data2;
                case MathItemType_et.oper_div:
                    return data1 / data2;
                case MathItemType_et.oper_mod:
                    return data1 % data2;
                case MathItemType_et.oper_power:
                    return Math.Pow(data1, data2);

                case MathItemType_et.oper_gt:
                    if(data1>data2) { return 1; } else { return 0; }
                case MathItemType_et.oper_lt:
                    if (data1 < data2) { return 1; } else { return 0; }
                case MathItemType_et.oper_ge:
                    if (data1 >= data2) { return 1; } else { return 0; }
                case MathItemType_et.oper_le:
                    if (data1 <= data2) { return 1; } else { return 0; }
                case MathItemType_et.oper_eq:
                    if (data1 == data2) { return 1; } else { return 0; }
                default:
                    ErrorCallback("MATH: unknown operator");
                    return double.NaN;
            }
        }

        public List<MathItem> ParseMathExpression(string expression, int orgIdx)
        {
            List<MathItem> array = new List<MathItem>();

            string tmpStr = "";

            int bracketLevel = 0;

            bool ok = true;

            int mode = 0; /* 0 - normal, 1 - variable start, 2 - variable name, 3 - variable num, 4 - mul/pwr , 5 - pwr , 6 - constVar*/

            for (int idx = 0; (idx < expression.Length + 1) && ok; idx++)
            {
                char c;
                if (idx < expression.Length)
                {
                    c = expression[idx];
                }
                else
                {
                    c = ';';
                }

                if (c == ' ') { continue; }

                if (mode == 3)
                {
                    if (c >= '0' && c <= '9')
                    {
                        tmpStr += c;
                    }
                    else
                    {
                        mode = 0;
                        MathItem item = new MathItem(MathItemType_et.doubleVar);
                        item.idx = GetGlobalIdx(tmpStr);
                        if(item.idx < 0)
                        {
                            ok = false;
                            ErrorCallback(orgIdx.ToString() + " Unknown global variable !" );
                        }
                        tmpStr = "";
                        array.Add(item);
                    }
                }
                else if (mode == 4)
                {
                    if (c == '*') { mode = 5; }
                    else
                    {
                        array.Add(new MathItem(MathItemType_et.oper_mul));
                        mode = 0;
                    }
                }
                else if (mode == 5)
                {
                    array.Add(new MathItem(MathItemType_et.oper_power));
                    mode = 0;
                }
                else if (mode == 6)
                {
                    if ((c >= '0' && c <= '9') || (c == '.') || (c == ','))
                    {
                        tmpStr += c;
                    }
                    else
                    {
                        MathItem item = new MathItem();
                        tmpStr = tmpStr.Replace('.', ',');
                        try
                        {
                            item.value = Convert.ToDouble(tmpStr);
                            item.type = MathItemType_et.constVar;
                        }
                        catch (FormatException)
                        {
                            item.value = 0;
                            item.type = MathItemType_et.constVar;
                            ok = false;
                            ErrorCallback(orgIdx.ToString() + " Number format error !");
                        }
                        catch (OverflowException)
                        {
                            item.value = 0;
                            item.type = MathItemType_et.constVar;
                            ok = false;
                            ErrorCallback(orgIdx.ToString() + " Number overflow error !");
                        }

                        tmpStr = "";
                        array.Add(item);
                        mode = 0;
                    }
                }

                if (mode == 0)
                {
                    switch (c)
                    {
                        case '[':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr,orgIdx))); tmpStr = ""; }
                            array.Add(new MathItem(MathItemType_et.bracketOpen));
                            bracketLevel++;
                            break;
                        case ']':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                            array.Add(new MathItem(MathItemType_et.bracketClose));
                            bracketLevel--;
                            break;
                        case '-':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                            array.Add(new MathItem(MathItemType_et.oper_sub));
                            break;
                        case '+':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                            array.Add(new MathItem(MathItemType_et.oper_add));
                            break;
                        case '/':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                            array.Add(new MathItem(MathItemType_et.oper_div));
                            break;
                        case '*':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                            mode = 4;
                            break;
                        case '#':
                            if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                            mode = 1;
                            tmpStr = "#";
                            break;
                        default:
                            if ((c >= '0' && c <= '9') || (c == ',') || (c == '.'))
                            {
                                if (tmpStr != "") { array.Add(new MathItem(GetOperFromStr(tmpStr, orgIdx))); tmpStr = ""; }
                                tmpStr = "" + c;
                                mode = 6;
                            }
                            else
                            {
                                tmpStr += c;
                            }

                            break;
                    }

                }
                else if (mode == 1)
                {
                    if (c == '<')
                    {
                        mode = 2;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        mode = 3;
                    }
                    else
                    {
                        ErrorCallback(orgIdx.ToString() + " MATH: invalid variable format");
                        ok = false;
                    }
                    tmpStr += c;
                }
                else if (mode == 2)
                {
                    tmpStr += c;
                    if (c == '>')
                    {
                        mode = 0;
                        MathItem item = new MathItem(MathItemType_et.doubleVar);
                        item.idx = GetVariableIdx(tmpStr);
                        if (item.idx < 0)
                        {
                            ok = false;
                            ErrorCallback(orgIdx.ToString() + " Unknown variable !");

                        }
                        array.Add(item);
                        tmpStr = "";
                    }
                }
            }

            if((array.Count == 0) || (bracketLevel != 0))
            {
                ok = false;
                ErrorCallback(orgIdx.ToString() + " MATH: bracket error");
            }

            if(ok)
            {
                ok = CheckExpressionSyntax(array, orgIdx);
            }

            if(ok)
            {
                return array;
            }
            else
            {
                return null;
            }
            
        }

        private bool CheckExpressionSyntax(List<MathItem> expression, int orgIdx)
        {
            bool ok = true;

            /* phase 1 - fix negative const syntax */

            if (expression.Count >= 2)
            {
                for (int idx = 0; idx < (expression.Count - 2); idx++)
                {
                    if ((expression[idx + 2].type == MathItemType_et.constVar) && (expression[idx + 1].type == MathItemType_et.oper_sub))
                    {
                        switch (expression[idx].type)
                        {
                            case MathItemType_et.constVar:
                            case MathItemType_et.doubleVar:
                            case MathItemType_et.bracketClose:
                                break;
                            default:
                                expression[idx + 2].value = -expression[idx + 2].value;
                                expression.RemoveAt(idx + 1);
                                break;
                        }
                    }
                }
            }

            /* phase 2 - check syntax */

            int v = expression.Count - 1;
            for (int idx = 0; idx < expression.Count; idx++)
            {
                switch (expression[idx].GetSyntaxGroup())
                {
                    case MathItem.SyntaxGrp_et.grp_var:
                        if (idx > 0 && expression[idx - 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_var) { ok = false; }
                        if (idx < v && expression[idx + 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_var) { ok = false; }
                        if (idx > 0 && expression[idx - 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_bracketClose) { ok = false; }
                        if (idx < v && expression[idx + 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_bracketOpen) { ok = false; }
                        break;
                    case MathItem.SyntaxGrp_et.grp_bracketOpen:
                        if (idx < v && expression[idx + 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_bracketClose) { ok = false; }
                        break;
                    case MathItem.SyntaxGrp_et.grp_bracketClose:
                        if (idx < v && expression[idx + 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_bracketOpen) { ok = false; }
                        break;
                    case MathItem.SyntaxGrp_et.grp_funcBracket:
                        if (idx < v && expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_bracketOpen) { ok = false; }
                        if (idx > 0 && expression[idx - 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_var) { ok = false; }
                        if (idx > 0 && expression[idx - 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_bracketClose) { ok = false; }
                        break;
                    case MathItem.SyntaxGrp_et.grp_funcTwoArg:
                        if (idx > 0)
                        {
                            if ((expression[idx - 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_var) &&
                               (expression[idx - 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_bracketClose))
                            {
                                ok = false;
                            }
                        }
                        if (idx < v)
                        {
                            if ((expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_var) &&
                               (expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_bracketOpen) &&
                               (expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_funcBracket))
                            {
                                ok = false;
                            }
                        }
                        break;

                    case MathItem.SyntaxGrp_et.grp_funcTwoArgStart:
                        if (idx > 0)
                        {
                            if ((expression[idx - 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_funcTwoArg) ||
                                (expression[idx - 1].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_funcTwoArgStart))
                            {
                                ok = false;
                            }
                        }
                        if (idx < v)
                        {
                            if ((expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_var) &&
                               (expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_bracketOpen) &&
                               (expression[idx + 1].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_funcBracket))
                            {
                                ok = false;
                            }
                        }
                        break;

                    default:
                        ok = false;
                        break;
                }
            }
            /* phase 3 - check syntax on start and end */

            if ((expression[0].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_bracketClose) ||
                (expression[0].GetSyntaxGroup() == MathItem.SyntaxGrp_et.grp_funcTwoArg))
            {
                ok = false;
            }

            if ((expression[v].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_bracketClose) &&
                (expression[v].GetSyntaxGroup() != MathItem.SyntaxGrp_et.grp_var))
            {
                ok = false;
            }

            if (ok == false)
            {
                ErrorCallback(orgIdx.ToString() + " MATH: syntax error");
            }

            return ok;
        }

        MathItemType_et GetOperFromStr(string stringItem, int orgIdx)
        {
            if (stringItem == "SIN")
            {
                return MathItemType_et.func_sin;
            }
            else if (stringItem == "COS")
            {
                return MathItemType_et.func_cos;
            }
            else if (stringItem == "TAN")
            {
                return MathItemType_et.func_tan;
            }
            if (stringItem == "ASIN")
            {
                return MathItemType_et.func_asin;
            }
            else if (stringItem == "ACOS")
            {
                return MathItemType_et.func_acos;
            }
            else if (stringItem == "ATAN")
            {
                return MathItemType_et.func_atan;
            }
            else if (stringItem == "SQRT")
            {
                return MathItemType_et.func_sqrt;
            }
            else if (stringItem == "MOD")
            {
                return MathItemType_et.oper_mod;
            }
            else if (stringItem == "AND")
            {
                return MathItemType_et.oper_and;
            }
            else if (stringItem == "OR")
            {
                return MathItemType_et.oper_or;
            }
            else if (stringItem == "XOR")
            {
                return MathItemType_et.oper_xor;
            }
            else if (stringItem == "EQ")
            {
                return MathItemType_et.oper_eq;
            }
            else if (stringItem == "NE")
            {
                return MathItemType_et.oper_ne;
            }
            else if (stringItem == "GT")
            {
                return MathItemType_et.oper_gt;
            }
            else if (stringItem == "GE")
            {
                return MathItemType_et.oper_ge;
            }
            else if (stringItem == "LT")
            {
                return MathItemType_et.oper_lt;
            }
            else if (stringItem == "LE")
            {
                return MathItemType_et.oper_le;
            }
            ErrorCallback(orgIdx.ToString() + " MATH: unknown literal");
            return MathItemType_et.unknown;
        }


        public double ResolveMathExpression(List<MathItem> arrayConst, int orgIdx)
        {
            List<MathItem> array = new List<MathItem>(arrayConst.Count);
            bool ok = true;

            foreach (var item in arrayConst)
            {
                array.Add(new MathItem(item));
            }

            /* fill all variables */
            foreach (var item in array)
            {
                if (item.type == MathItemType_et.doubleVar)
                {
                    item.value = ReadVariable(item.idx);
                    if(double.IsNaN(item.value))
                    {
                        ErrorCallback(orgIdx.ToString() + " MATH: variable read failed");
                        ok = false; break;
                    }
                    item.type = MathItemType_et.constVar;
                }
            }

            while( (array.Count > 1) && ok)
            {
                int startIdx = -1;
                int stopIdx = array.Count;
                /* part 1 - get upper brackets */
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].type == MathItemType_et.bracketOpen)
                    {
                        startIdx = i;
                    }
                    if (array[i].type == MathItemType_et.bracketClose)
                    {
                        stopIdx = i;
                        break;
                    }
                }

                if (array.Count == 2)
                {
                    /* -X  or +X format case */

                    if (array[1].type == MathItemType_et.constVar)
                    {

                        if (array[0].type == MathItemType_et.oper_add)
                        {
                            array[1].value = array[1].value;
                            array.RemoveAt(0);
                        }
                        else if (array[0].type == MathItemType_et.oper_sub)
                        {
                            array[1].value = -array[1].value;
                            array.RemoveAt(0);
                        }
                        else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                    }
                    else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                }

                else if (((startIdx >= 0) && (stopIdx >= 0)) || (startIdx < 0))
                {
                    /* part 2 - find operator with the highest priority and resolve it */


                    if ((stopIdx - startIdx) == 2)
                    {
                        if (array[startIdx + 1].type == MathItemType_et.constVar)
                        {
                            double argVal = array[startIdx + 1].value;

                            if ((startIdx > 0) && (array[startIdx - 1].GetPriority() == 1)) /* function resolve */
                            {
                                double newVal = CalcFunction(argVal, array[startIdx - 1].type);
                                if (double.IsNaN(newVal)) { ok = false; }
                                array[startIdx + 1].value = newVal;
                                array.RemoveAt(startIdx + 2);
                                array.RemoveAt(startIdx);
                                array.RemoveAt(startIdx - 1);
                            }
                            else /* bracket resolve */
                            {
                                array.RemoveAt(startIdx + 2);
                                array.RemoveAt(startIdx);
                            }
                        }
                        else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                    }
                    else if ((stopIdx - startIdx) > 2)
                    {
                        int maxIdx = -1;
                        int maxPrio = -1;
                        for (int i = startIdx + 1; i < stopIdx; i++)
                        {
                            if (array[i].GetPriority() > maxPrio)
                            {
                                maxPrio = array[i].GetPriority();
                                maxIdx = i;
                            }
                        }

                        if ((maxPrio > 1) && (maxIdx < stopIdx) && (maxIdx > startIdx))
                        {

                            if ((array[maxIdx].type == MathItemType_et.oper_sub) && (maxIdx == 0 ||(array[maxIdx - 1].type != MathItemType_et.constVar)))
                            {
                                if (array[maxIdx + 1].type == MathItemType_et.constVar)
                                {
                                    array[maxIdx + 1].value = -array[maxIdx + 1].value;
                                    array.RemoveAt(maxIdx);
                                }
                                else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                            }
                            else if ((array[maxIdx].type == MathItemType_et.oper_add) && (maxIdx == 0 || (array[maxIdx - 1].type != MathItemType_et.constVar)))
                            {
                                if (array[maxIdx + 1].type == MathItemType_et.constVar)
                                {
                                    array[maxIdx + 1].value = array[maxIdx + 1].value;
                                    array.RemoveAt(maxIdx);
                                }
                                else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                            }
                            else
                            {
                                if ((array[maxIdx + 1].type == MathItemType_et.constVar) && (array[maxIdx - 1].type == MathItemType_et.constVar))
                                {
                                    double newVal = CalcOperator(array[maxIdx - 1].value, array[maxIdx + 1].value, array[maxIdx].type);
                                    if (double.IsNaN(newVal)) { ok = false; }
                                    array[maxIdx - 1].value = newVal;
                                    array.RemoveAt(maxIdx + 1);
                                    array.RemoveAt(maxIdx);
                                }
                                else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                            }

                            



                        }
                        else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                    }
                    else { ErrorCallback(orgIdx.ToString() + " MATH: syntax error"); ok = false; }
                }
                else { ErrorCallback(orgIdx.ToString() +  " MATH: syntax error"); ok = false; }
            }
            if (ok)
            {
                return array[0].value;
            }
            else
            {
                return double.NaN;
            }
        }
    }
}
