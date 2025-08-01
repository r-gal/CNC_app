using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace CNC3
{
    public struct Word_st
    {
        public char c;
        public double value;
    }

    public class VariableArrayItem
    {
        public string name {  get; set; }   
        public double value { get; set; }
    }

    

    

    public enum GcodeType_et
    {
        GCTYPE_variableWrite,
        GCTYPE_G,
        GCTYPE_M,
        GCTYPE_O
    }

    public class Gcode_param
    {
        public char name;
        public List<MathItem> mathArray;
    }

    public class GcodeItem
    {
        public GcodeType_et type;
        public int orgIdx;
    }

    public class VarWrite : GcodeItem 
    {
        public VarWrite()
        {
            type = GcodeType_et.GCTYPE_variableWrite;
        }

        public int varIdx;
        public List<MathItem> mathArray;
    }


    public class Code_GM : GcodeItem
    {
        
        public Code_GM(GcodeType_et type_)
        {
            type = type_;
            paramArray = new List<Gcode_param>();
        }
        public List<Gcode_param> paramArray;
        public int code;
    }

    public enum CodeOType_et
    {
        CodeO_while,
        CodeO_endwhile,
        CodeO_do,
        CodeO_enddo,
        CodeO_repeat,
        CodeO_endrepeat,
        CodeO_if,
        CodeO_else,
        CodeO_elseif,
        CodeO_endif,
        CodeO_sub,
        CodeO_endsub,
        CodeO_call,
        CodeO_return,
        CodeO_unknown
    }

    public class Code_O : GcodeItem
    {
        public CodeOType_et code;
        public Code_O next;
        public Code_O parent;
        public Subprogram sub;
        public bool used; /* for if/elseif usage */
        public int cnt; /* for repeat uage */

        public List< List<MathItem>> mathArray;
        public int seqNo;
        public int idx;
        public Code_O(CodeOType_et code_)
        {
            code = code_;
            type = GcodeType_et.GCTYPE_O;
            next = null;
            parent = null;
            mathArray = new List<List<MathItem>>();
        }
    }

    public class Subprogram
    {
        public int seqNo;
        public bool parsed;
        public gCodeCompMath math;

        public int execIdx;


        public List<GcodeItem> codeArray = new List<GcodeItem>();

        public Subprogram(int seqNo_)
        {
            this.seqNo = seqNo_;
            parsed = false;
            math = new gCodeCompMath();            
        }
    }
    public class Call
    {
        public int returnIdx;
        public int returnExecIdx;
        public int actIdx;
        public double[] storedParams;
        public int argsNo;

    }

    public class cGodeCompiller
    {
        public delegate void CallbackErrorCallback(string errorMsg);
        public static event CallbackErrorCallback ErrorCallback;

        List<Subprogram> subprograms = new List<Subprogram>();
        List<Call> callStack = new List<Call>();
        Subprogram actSubprogram = null;

        List<GcodeItem> baseCodeArray = new List<GcodeItem>();
        List<GcodeItem> codeArray = null;


        gCodeCompMath baseMath = new gCodeCompMath();
        gCodeCompMath math;

        List<Code_O> code_Os = new List<Code_O>();

        Subprogram parsingSub = null;

        bool endFound = false;

        public cGodeCompiller()
        {
            math = baseMath;
            codeArray = baseCodeArray;
        }

        List<Word_st[]> programList = new List<Word_st[]>();


        public void Init()
        {
            
        }

        public List<Word_st[]> GetProgramList()
        {
            return programList;
        }

        public class PreProcItem
        {
            public string cmdLine;
            public int orgIdx;
        }

        public bool RunCompilation(string[] codeFile)
        {


            int lines = codeFile.Length; /* GetNoOfLinesCallback();*/

            //List<string> preprocesed = new List<string>();
            List<PreProcItem> preprocesed = new List<PreProcItem>();

            string tmpLine = "";

            bool ok = true;

            MainClass.SetProgress(0,"Compiling");


            for (int i = 0; (i < lines) && ok; i++)
            {
                /* preparation */

                /* phase 1 - deletion comments */
                string line = codeFile[i];/*GetLineCallback(i);*/

                line = System.Text.RegularExpressions.Regex.Replace(line, "\\(.*\\)", "");
                line = System.Text.RegularExpressions.Regex.Replace(line, ";.*", "");

                /* phase 2 - delete spaces and change to uppercase */
                line = line.Trim();
                line = line.ToUpper();

                /* phase 3 - merge lines for one command */

                if(line.Length > 0) 
                {

                    if (i > 0)
                    {
                        switch (line[0])
                        {
                            case '#':
                            case 'G':
                            case 'M':
                            case 'O':
                                PreProcItem itm = new PreProcItem();
                                itm.cmdLine = tmpLine;
                                itm.orgIdx = i;
                                preprocesed.Add(itm);
                                tmpLine = "";
                                
                                break;
                            default:
                                tmpLine += " ";
                                break;
                        }
                    }
                    tmpLine += line;
                }
                /* phase 3 - selection type */

                MainClass.SetProgress((50*i)/lines);
            }
            /* add last line */
            if( tmpLine != "")
            {
                PreProcItem itm = new PreProcItem();
                itm.cmdLine = tmpLine;
                itm.orgIdx = -1;
                preprocesed.Add(itm);
            }
            /* phase 4 - parse lines to codeArray */

            for (int i = 0; (i < preprocesed.Count) && ok; i++)
            {
                ok = ParseLine(preprocesed[i]);

                MainClass.SetProgress(50 + ((50 * i) / preprocesed.Count));
            }

            if(ok)
            {
                if(endFound == false)
                {
                    ok = false;
                    ErrorCallback("Missing M2 or M30\n");
                }
            }

            if(ok)
            {
                ErrorCallback("Compilation Success\n");
            }
            MainClass.SetProgress(100);

            return ok;
        }

        int execIdx;
        
        public Word_st[] ExecuteLine()
        {
            
            //int idx = execIdx;
            bool ok = true;

            while ((execIdx < codeArray.Count) && ok)
            {
                MainClass.SetProgress(execIdx * 100 / codeArray.Count, "Run");
                double result = 0;
                int orgIdx = codeArray[execIdx].orgIdx;
                switch (codeArray[execIdx].type)
                {
                    case GcodeType_et.GCTYPE_variableWrite:
                        VarWrite item_W = (VarWrite)codeArray[execIdx];

                        double val = math.ResolveMathExpression(item_W.mathArray, orgIdx);
                        if (double.IsNaN(val) == false)
                        {
                            if (math.WriteWariableByIdx(item_W.varIdx, val) == -1)
                            {
                                ok = false;
                            }
                            execIdx++;
                        }
                        else
                        {
                            ok = false;
                        }
                        break;
                    case GcodeType_et.GCTYPE_G:
                    case GcodeType_et.GCTYPE_M:
                        Code_GM item_G = (Code_GM)codeArray[execIdx];
                        //string line;
                        Word_st[] lineArray = new Word_st[2 + item_G.paramArray.Count];

                        if (item_G.type == GcodeType_et.GCTYPE_G)
                        {
                            //line = "G" + item_G.code.ToString();
                            lineArray[0].c = 'G';
                            lineArray[0].value = item_G.code;

                        }
                        else
                        {
                            //line = "M" + item_G.code.ToString();
                            lineArray[0].c = 'M';
                            lineArray[0].value = item_G.code;
                        }
                        lineArray[1].c = '@';
                        lineArray[1].value = item_G.orgIdx;

                        int i = 2;
                        foreach (var item in item_G.paramArray)
                        {
                            double parVal = math.ResolveMathExpression(item.mathArray, orgIdx);
                            if (double.IsNaN(parVal) == false)
                            {
                                //line += " " + item.name + parVal.ToString();

                                lineArray[i].c = item.name;
                                lineArray[i].value = parVal;
                                i++;
                            }
                            else
                            {
                                ok = false;
                            }

                        }
                        //outputTextBox.AppendText(line + Environment.NewLine);
                        //programList.Add(lineArray);
                        execIdx++;
                        return lineArray; 
                    case GcodeType_et.GCTYPE_O:
                        Code_O item_O = (Code_O)codeArray[execIdx];
                        switch (item_O.code)
                        {
                            case CodeOType_et.CodeO_if:
                            case CodeOType_et.CodeO_elseif:
                                if (item_O.code == CodeOType_et.CodeO_if) { item_O.parent.used = false; }
                                if (item_O.parent.used == false)
                                {
                                    result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                    if (double.IsNaN(result)) { ok = false; }
                                }
                                if (0 == result)
                                {
                                    Code_O tmp = item_O.next;
                                    if (tmp != null)
                                    {
                                        execIdx = tmp.idx;
                                    }
                                    else
                                    {
                                        ErrorCallback("O_code IF-ELSE failed !");
                                        ok = false;
                                    }
                                }
                                else
                                {
                                    item_O.parent.used = true;
                                }
                                break;
                            case CodeOType_et.CodeO_else:
                                if (item_O.parent.used == true)
                                {
                                    Code_O tmp = item_O.next;
                                    if (tmp != null)
                                    {
                                        execIdx = tmp.idx;
                                    }
                                    else
                                    {
                                        ErrorCallback("O_code IF-ELSE failed !");
                                        ok = false;
                                    }
                                }
                                break;
                            case CodeOType_et.CodeO_endif:
                                break;

                            case CodeOType_et.CodeO_while:
                                result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                if (double.IsNaN(result)) { ok = false; }
                                else
                                {
                                    if (result == 0) { execIdx = item_O.next.idx +1 ; }
                                }
                                break;
                            case CodeOType_et.CodeO_endwhile:
                                execIdx = item_O.parent.idx;
                                break;
                            case CodeOType_et.CodeO_do:
                                break;
                            case CodeOType_et.CodeO_enddo:
                                result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                if (double.IsNaN(result)) { ok = false; }
                                else
                                {
                                    if (result != 0) { execIdx = item_O.parent.idx; }
                                }
                                break;
                            case CodeOType_et.CodeO_repeat:
                                result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                if (double.IsNaN(result))
                                {
                                    ok = false;
                                    item_O.cnt = 0;
                                }
                                else
                                {
                                    item_O.cnt = ((int)result);
                                }
                                if (item_O.cnt == 0) { execIdx = item_O.next.idx; }
                                break;
                            case CodeOType_et.CodeO_endrepeat:
                                item_O.parent.cnt--;
                                if (item_O.parent.cnt > 0)
                                {
                                    execIdx = item_O.parent.idx + 1;
                                }
                                break;
                            case CodeOType_et.CodeO_call:
                                int seqNo = item_O.seqNo;
                                Subprogram sub = subprograms.Find(x => x.seqNo == seqNo);
                                sub.execIdx = 0;

                                Call call = new Call();
                                call.actIdx = seqNo;
                                if (callStack.Count == 0)
                                {
                                    call.returnIdx = -1;
                                }
                                else
                                {
                                    call.returnIdx = callStack.Last().actIdx;
                                }
                                call.returnExecIdx = execIdx;

                                int argsNo = item_O.mathArray.Count;
                                if (argsNo > 0)
                                {
                                    call.storedParams = new double[argsNo];
                                    for(int argIdx = 0;argIdx< argsNo;argIdx++)
                                    {
                                        result = math.ResolveMathExpression(item_O.mathArray[argIdx], orgIdx);
                                        if (double.IsNaN(result))
                                        {
                                            ok = false;                                            
                                        }
                                        else
                                        {
                                            call.storedParams[argIdx ] = gCodeCompMath.globalArray[argIdx+1];
                                            gCodeCompMath.globalArray[argIdx + 1] = result;
                                        }
                                    }
                                }
                                call.argsNo = argsNo;

                                callStack.Add(call);
                                codeArray = sub.codeArray;
                                math = sub.math;
                                execIdx = -1; /* will be counted up to 0 in next steps */

                                break;

                            case CodeOType_et.CodeO_return:


                                Call actCall = callStack.Last();

                                execIdx = actCall.returnExecIdx;

                                if (actCall.returnIdx >= 0)
                                {
                                    Subprogram retSub = subprograms.Find(x => x.seqNo == actCall.returnIdx);
                                    codeArray = retSub.codeArray;
                                    math = retSub.math;
                                }
                                else
                                {
                                    codeArray = baseCodeArray;
                                    math = baseMath;
                                }

                                for (int paramIdx = 0; paramIdx < actCall.argsNo; paramIdx++)
                                {
                                    gCodeCompMath.globalArray[paramIdx+1] = actCall.storedParams[paramIdx];
                                }

                                callStack.Remove(actCall);

                                break;
                            default:
                                //outputTextBox.AppendText(item_O.code.ToString() + Environment.NewLine);
                                break;
                        }
                        execIdx++;
                        break;
                    default:
                        execIdx++;
                        break;
                }
            }
            return null;
        }
        public bool RunExecution()
        {


            int idx = 0;
            bool ok = true;

            while((idx < codeArray.Count) && ok)
            {
                double result = 0;
                int orgIdx = codeArray[idx].orgIdx;
                switch (codeArray[idx].type)
                {
                    case GcodeType_et.GCTYPE_variableWrite:
                        VarWrite item_W = (VarWrite)codeArray[idx];

                        double val = math.ResolveMathExpression(item_W.mathArray, orgIdx);
                        if(double.IsNaN(val)  == false)
                        {
                            if(math.WriteWariableByIdx(item_W.varIdx, val) == -1)
                            {
                                ok = false;
                            }
                            idx++;
                        }
                        else
                        {
                            ok = false;
                        }                        
                        break;
                    case GcodeType_et.GCTYPE_G:
                    case GcodeType_et.GCTYPE_M:
                        Code_GM item_G = (Code_GM)codeArray[idx];
                        //string line;
                        Word_st[] lineArray = new Word_st[1+ item_G.paramArray.Count];

                        if (item_G.type == GcodeType_et.GCTYPE_G)
                        {
                            //line = "G" + item_G.code.ToString();
                            lineArray[0].c = 'G';
                            lineArray[0].value = item_G.code;

                        }
                        else
                        {
                            //line = "M" + item_G.code.ToString();
                            lineArray[0].c = 'M';
                            lineArray[0].value = item_G.code;
                        }
                        int i = 1;
                        foreach(var item in item_G.paramArray)
                        {
                            double parVal = math.ResolveMathExpression(item.mathArray, orgIdx);
                            if (double.IsNaN(parVal ) == false)
                            {
                                //line += " " + item.name + parVal.ToString();

                                lineArray[i].c = item.name;
                                lineArray[i].value = parVal;
                                i++;
                            }
                            else
                            {
                                ok = false;
                            }
                            
                        }
                        //outputTextBox.AppendText(line + Environment.NewLine);
                        programList.Add(lineArray);
                        idx++;
                        break;
                    case GcodeType_et.GCTYPE_O:
                        Code_O item_O = (Code_O)codeArray[idx];
                        switch(item_O.code)
                        {
                            case CodeOType_et.CodeO_if:
                            case CodeOType_et.CodeO_elseif:
                                if(item_O.code == CodeOType_et.CodeO_if) { item_O.parent.used = false; }                                
                                if (item_O.parent.used == false)
                                {
                                    result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                    if (double.IsNaN(result)) { ok = false; }
                                }
                                if (0 == result)
                                {                                    
                                    Code_O tmp = item_O.next;
                                    if(tmp != null)
                                    { 
                                        idx = tmp.idx;
                                    }
                                    else
                                    {
                                        ErrorCallback("O_code IF-ELSE failed !");
                                        ok = false;
                                    }  
                                }
                                else
                                {
                                    item_O.parent.used = true;
                                }
                                break;
                            case CodeOType_et.CodeO_else:
                                if (item_O.parent.used == true)
                                {
                                    Code_O tmp = item_O.next;
                                    if (tmp != null)
                                    {
                                        idx = tmp.idx;
                                    }
                                    else
                                    {
                                        ErrorCallback("O_code IF-ELSE failed !");
                                        ok = false;
                                    }
                                }
                                break;
                            case CodeOType_et.CodeO_endif:
                                break;

                            case CodeOType_et.CodeO_while:
                                result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                if (double.IsNaN(result)) { ok = false; }
                                else
                                {
                                    if(result == 0) {  idx = item_O.next.idx + 1; }
                                }     
                                break;
                            case CodeOType_et.CodeO_endwhile:
                                idx = item_O.parent.idx;
                                break;
                            case CodeOType_et.CodeO_do:
                                break;
                            case CodeOType_et.CodeO_enddo:
                                result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                if (double.IsNaN(result)) { ok = false; }
                                else
                                {
                                    if (result != 0) { idx = item_O.parent.idx; }
                                }
                                break;
                            case CodeOType_et.CodeO_repeat:
                                result = math.ResolveMathExpression(item_O.mathArray[0], orgIdx);
                                if (double.IsNaN(result)) 
                                {
                                    ok = false;
                                    item_O.cnt = 0;
                                }
                                else
                                {
                                    item_O.cnt = ((int)result);
                                }
                                if(item_O.cnt == 0) { idx = item_O.next.idx; }
                                break;
                            case CodeOType_et.CodeO_endrepeat:
                                item_O.parent.cnt--;
                                if (item_O.parent.cnt > 0)
                                {                                    
                                    idx = item_O.parent.idx + 1;
                                }                     
                                break;
                            default:
                                //outputTextBox.AppendText(item_O.code.ToString() + Environment.NewLine);
                                break;
                        }                        
                        idx++;
                        break;
                    default:
                        idx++;
                        break;  
                }
            }
            Word_st[] endLine = new Word_st[1];
            endLine[0].c = 'E';
            endLine[0].value = 0;
            programList.Add(endLine);

            return ok;
        }

        public void PrintProgramList(ListBox list)
        {
            list.Items.Clear();
            foreach(var item in programList)
            {
                string line = "";
                foreach(var item_O in item) 
                {
                    line += item_O.c + item_O.value.ToString() + " ";                    
                }
                list.Items.Add(line);
            }
        }

        public void ClearAll()
        {
            /* clear   variableArray*/
            math.Clear();

            gCodeCompMath.ClearStatic();

            /* clear codeArray*/
            codeArray.Clear();

            code_Os.Clear();
            parsingSub = null;

            programList.Clear();

            execIdx = 0;

            subprograms.Clear();
            callStack.Clear();

            endFound = false;



        }

        private void PrintError(int line,string err)
        {
            ErrorCallback("Comp: Line " + line.ToString() + ":" + err);
        }


        private bool ParseLine(PreProcItem line)
        {
            bool ok = true;
            if(line.cmdLine.Length > 0)
            {
                switch (line.cmdLine[0])
                {
                    case '#':
                        ok = ParseVariableWrite(line) ;
                        break;
                    case 'G':
                    case 'M':
                        ok = ParseCodeGM(line);
                        break;
                    case 'O':
                        ok = ParseCodeO(line);
                        break;
                    default:
                        PrintError(line.orgIdx,"invalid line !" );
                        ok = false;
                        break;

                }
            }
            return ok;
        }



        private bool ParseVariableWrite(PreProcItem line)
        {
            bool ok = true; 
            string[] varData = line.cmdLine.Split('=');
            if (varData.Length == 2)
            {
                string varName = varData[0];
                varName = varName.Trim();
                string varValue = varData[1].Trim();
                varValue = varValue.Trim();

                int varIdx = -1;

                varIdx = math.WriteWariableByStr(varName, 0);

                if (varIdx == -1)
                {
                    ok = false;
                    PrintError(line.orgIdx, "Format error !");
                }


                if (ok)
                {
                    VarWrite item = new VarWrite();
                    item.mathArray = math.ParseMathExpression(varValue,line.orgIdx);
                    if (item.mathArray != null)
                    {
                        item.varIdx = varIdx;
                        item.orgIdx = line.orgIdx;
                        codeArray.Add(item);
                    }
                    else
                    {
                        ok = false;
                    }
                }
            }
            else
            {
                PrintError(line.orgIdx,"Format error !");
                ok = false;
            }
            return ok;
        }

        class tab1_st
        {
            public string text;
            public int priority;
            public int numeral;
            public bool parameters;
            public tab1_st()
            {
                text = "";
                priority = -1;
            }
        }

        private bool ParseCodeGM(PreProcItem lineItm)
        {
            bool ok = true;
            /*
            string pattern = @"([A-Z][0-9.]+)";
            Regex regex = new Regex(pattern);        
            string[] substrings = regex.Split(line);*/

            string line = lineItm.cmdLine;
            
            List<tab1_st> substrings = new List<tab1_st>();

            tab1_st act = new tab1_st();
            int bracketLevel = 0;

            for (int i = 0; i <= line.Length; i++)
            {
                char c;
                if (i < line.Length) { c = line[i]; } else { c = ';'; }

                if (i == line.Length)
                {
                    if (act.text != "")
                    {
                        substrings.Add(act);
                        act = new tab1_st();
                    }
                }
                else if ((c >= 'A' && c <= 'Z') && (bracketLevel == 0))
                {
                    if (act.text != "")
                    {
                        substrings.Add(act);
                        act = new tab1_st();
                    }
                }
                else if ((c == '[') || (c == '<'))
                {
                    bracketLevel++;
                }
                else if ((c == ']') || (c == '>'))
                {
                    bracketLevel--;
                }

                if (i < line.Length)
                { 
                    act.text += c;
                }

            }
            if (act.text != "")
            {
                substrings.Add(act);
            }

            if (bracketLevel == 0)
            {
                /* set priorites for GCODES */
                bool paramCodeFound = false;
                foreach (var stritem in substrings)
                {
                    stritem.text = stritem.text.Trim();
                    if ((stritem.text[0] == 'G') || (stritem.text[0] == 'M'))
                    {
                        if ((stritem.text.Length >= 4) && (stritem.text[stritem.text.Length-2] == '.'))
                        {
                            stritem.text = stritem.text.Remove(stritem.text.Length - 2,1);
                        }                        
                        else
                        {
                            stritem.text = stritem.text + '0';
                        }
                        try
                        {
                            stritem.numeral = Convert.ToUInt16(stritem.text.Substring(1));
                        }
                        catch (FormatException)
                        {
                            PrintError(lineItm.orgIdx, "Number format error !");
                            ok = false;
                        }
                        catch (OverflowException)
                        {
                            PrintError(lineItm.orgIdx, "Number format error !");
                            ok = false;
                        }

                        if (ok)
                        {

                            if (stritem.text[0] == 'G')
                            {
                                switch (stritem.numeral)
                                {
                                    case 530:
                                    case 00:
                                    case 10:
                                    case 20:
                                    case 21:
                                    case 30:
                                    case 31:
                                    case 382:
                                    case 383:
                                    case 384:
                                    case 385:
                                    case 800:
                                    case 810:
                                    case 820:
                                    case 830:
                                        stritem.priority = 1; break;
                                    case 280:
                                    case 281:
                                    case 300:
                                    case 301:
                                    case 100:
                                    case 520:
                                    case 920:
                                    case 921:
                                    case 922:
                                    case 923:
                                    case 940:
                                        stritem.priority = 2; break;
                                    case 980:
                                    case 990:
                                        stritem.priority = 3; break;
                                    case 900:
                                    case 910:
                                    case 901:
                                    case 911:
                                        stritem.priority = 4; break;
                                    case 610:
                                    case 640:
                                        stritem.priority = 5; break;
                                    case 540:
                                    case 550:
                                    case 560:
                                    case 570:
                                    case 580:
                                    case 590:
                                    case 591:
                                    case 592:
                                    case 593:
                                        stritem.priority = 6; break;
                                    case 430:
                                    case 433: /* surface offset init */
                                    case 434: /* surface offset set point */
                                    case 435: /* surface offset activate */
                                    case 436: /* surface offset deactivate */

                                    case 490:
                                        stritem.priority = 7; break;
                                    case 400:
                                    case 410:
                                    case 420:
                                        stritem.priority = 8; break;
                                    case 200:
                                    case 210:
                                        stritem.priority = 9; break;
                                    case 170:
                                    case 180:
                                    case 190:
                                        stritem.priority = 10; break;
                                    case 40:
                                        stritem.priority = 11; break;
                                    default:
                                        /* TODO - error : unsupported GCODE */
                                        ok = false;
                                        PrintError(lineItm.orgIdx, "Unsupported GCODE G" + stritem.numeral.ToString() + "!" );
                                        stritem.priority = 0;
                                        break;
                                }

                                switch (stritem.numeral)
                                {
                                    case 00:
                                    case 10:
                                    case 20:
                                    case 21:
                                    case 30:
                                    case 31:
                                    case 40:
                                    case 410:
                                    case 420:
                                    case 100:
                                    case 281:
                                    case 301:
                                    case 920:
                                    case 530:
                                    case 382: 
                                    case 383:
                                    case 384:
                                    case 385:
                                    case 433:
                                    case 434:
                                    case 810:
                                    case 820:
                                    case 830:
                                        stritem.parameters = true;
                                        break;
                                    default:
                                        stritem.parameters = false;
                                        break;
                                }
                            }
                            else if (stritem.text[0] == 'M')
                            {
                                stritem.priority = 0;
                                switch (stritem.numeral)
                                {
                                    case 30:
                                    case 40:
                                    case 60:

                                        stritem.parameters = true;
                                        break;
                                    default:
                                        stritem.parameters = false;
                                        break;
                                }
                            }


                            if (stritem.parameters)
                            {
                                if (paramCodeFound)
                                {
                                    ok = false;
                                    PrintError(lineItm.orgIdx, "Only one command with parameter allowed in one line !" );
                                }
                                paramCodeFound = stritem.parameters;
                            }
                        }
                    }
                }

                if (ok)
                {
                    int maxPrio = -1;

                    do
                    {
                        maxPrio = -1;
                        int maxIdx = 0;
                        for (int i = 0; i < substrings.Count; i++)
                        {
                            if (substrings[i].priority > maxPrio)
                            {
                                maxIdx = i;
                                maxPrio = substrings[i].priority;
                            }
                        }
                        Code_GM item;
                        if (substrings[maxIdx].text[0] == 'G')
                        {
                            item = new Code_GM(GcodeType_et.GCTYPE_G);
                        }
                        else
                        {
                            item = new Code_GM(GcodeType_et.GCTYPE_M);
                        }

                        item.code = substrings[maxIdx].numeral;

                        if (maxPrio >= 0)
                        {
                            if (substrings[maxIdx].parameters)
                            {
                                foreach (var stritem in substrings)
                                {
                                    if (stritem.priority == -1)
                                    {
                                        Gcode_param par = new Gcode_param();
                                        par.name = stritem.text[0];
                                        par.mathArray = math.ParseMathExpression(stritem.text.Substring(1), lineItm.orgIdx);
                                        if (par.mathArray != null)
                                        {                                            
                                            item.paramArray.Add(par);
                                        }
                                        else
                                        { 
                                            ok = false;
                                        }
                                    }
                                }
                            }
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                            substrings.RemoveAt(maxIdx);
                            if(actSubprogram == null && item.type == GcodeType_et.GCTYPE_M && (item.code == 20 || item.code == 300 ))
                            {
                                endFound = true;
                            }


                        }

                    } while ((maxPrio > 0) && substrings.Count > 0 &&ok);
                }
            }
            else
            {
                PrintError(lineItm.orgIdx, "Brackets error !");
                ok = false;
            }
            return ok;
        }


        private bool ParseCodeO(PreProcItem lineItm)
        {
            bool ok = true;

            string line = lineItm.cmdLine;

            List<string> paramStrings = new List<string>();

            string actStr = "";

            int seqNo = -1;

            int phase = 0; /* 0-seqNo, 1-command, 2-expressions */

            int bracketLevel = 0;

            CodeOType_et code = CodeOType_et.CodeO_unknown;
           

            for (int i = 1; i <= line.Length; i++)
            {
                char c;
                if(i < line.Length) { c = line[i];} else
                { c = ';'; }

                if (c == ' ') { continue;}

                if (phase == 0)
                {
                    if (c >= '0' && c <= '9')
                    {
                        actStr += c;
                    }
                    else
                    {
                        phase = 1;
                        try
                        {
                            seqNo = Convert.ToUInt16(actStr);
                        }
                        catch (FormatException)
                        {
                            PrintError(lineItm.orgIdx, "O_code Number format error !");
                            ok = false;
                        }
                        catch (OverflowException)
                        {
                            PrintError(lineItm.orgIdx, "O_code Number format error !");
                            ok = false;
                        }
                        actStr = "" + c;
                    }
                }
                else if (phase == 1)
                {
                    if (c >= 'A' && c <= 'Z')
                    {
                        actStr += c;
                    }
                    else if(c == '[' || c == ';')
                    {
                        if (actStr.Length == 2 && actStr == "IF") { code = CodeOType_et.CodeO_if; }
                        else if (actStr.Length == 6 && actStr == "ELSEIF") { code = CodeOType_et.CodeO_elseif; }
                        else if (actStr.Length == 4 && actStr == "ELSE") { code = CodeOType_et.CodeO_else;  }
                        else if (actStr.Length == 5 && actStr == "ENDIF") { code = CodeOType_et.CodeO_endif; }
                        else if (actStr.Length == 2 && actStr == "DO") { code = CodeOType_et.CodeO_do; }
                        else if (actStr.Length == 5 && actStr == "WHILE") { code = CodeOType_et.CodeO_while;   }
                        else if (actStr.Length == 8 && actStr == "ENDWHILE") { code = CodeOType_et.CodeO_endwhile;  }
                        else if (actStr.Length == 6 && actStr == "REPEAT") { code = CodeOType_et.CodeO_repeat;  }
                        else if (actStr.Length == 9 && actStr == "ENDREPEAT") { code = CodeOType_et.CodeO_endrepeat; }
                        else if (actStr.Length == 3 && actStr == "SUB") { code = CodeOType_et.CodeO_sub; }
                        else if (actStr.Length == 6 && actStr == "ENDSUB") { code = CodeOType_et.CodeO_endsub; }
                        else if (actStr.Length == 4 && actStr == "CALL") { code = CodeOType_et.CodeO_call; }
                        else if (actStr.Length == 6 && actStr == "RETURN") { code = CodeOType_et.CodeO_return; }
                        else { code = CodeOType_et.CodeO_unknown; }

                        phase = 2;
                        bracketLevel = 1;
                        if (i < line.Length)
                        {
                            actStr = "" + c;
                        }
                        else
                        {
                            actStr = "";
                        }
                    }
                    else
                    {
                        ok = false;
                    }
                }
                else
                {
                    if (i < line.Length)
                    {
                        actStr += c;
                        if (c == '[')
                        {
                            bracketLevel++;
                        }
                        else if(c == ']')
                        {
                            bracketLevel--;
                            if(bracketLevel == 0)
                            {
                                paramStrings.Add(actStr);
                                actStr = "";
                            }
                        }                        
                    }
                }
            }

            Code_O item = new Code_O(code);
            item.seqNo = seqNo;
            List<MathItem> expression = null;
            if (ok)
            {
                switch(code)
                {
                    case CodeOType_et.CodeO_if:
                    case CodeOType_et.CodeO_elseif:
                    case CodeOType_et.CodeO_repeat:
                    case CodeOType_et.CodeO_while:                        
                        if (paramStrings.Count == 1)
                        {
                            expression = math.ParseMathExpression(paramStrings[0],lineItm.orgIdx);
                        }                                                 
                        if(expression == null)
                        {
                            ok = false; 
                        }
                        else
                        {
                            item.mathArray.Add(expression );
                        }
                        break;
                    case CodeOType_et.CodeO_do:
                    case CodeOType_et.CodeO_else:
                    case CodeOType_et.CodeO_endif:
                    case CodeOType_et.CodeO_endrepeat:
                    case CodeOType_et.CodeO_sub:
                    case CodeOType_et.CodeO_endwhile:
                        if(paramStrings.Count != 0)
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code format error");
                        }
                        break;
                    case CodeOType_et.CodeO_endsub:
                    case CodeOType_et.CodeO_return:
                        if(paramStrings.Count > 1) 
                        {
                            PrintError(lineItm.orgIdx, "O_code too many parameters");
                            ok = false; 
                        }
                        else if (paramStrings.Count == 1)
                        {
                            expression = math.ParseMathExpression(paramStrings[0], lineItm.orgIdx);
                            if (expression == null)
                            {
                                ok = false;
                            }
                            else
                            {
                                item.mathArray.Add(expression);
                            }
                        }
                        break;
                    case CodeOType_et.CodeO_call:
                        for(int j=0;j< paramStrings.Count; j++)    
                        {
                            expression = math.ParseMathExpression(paramStrings[j], lineItm.orgIdx);
                            if (expression == null)
                            {
                                ok = false;
                            }
                            else
                            {
                                item.mathArray.Add(expression);
                            }
                        }
                        break;
                         
                    default:
                        ok = false;
                        PrintError(lineItm.orgIdx, "Unknown O_code!");
                        break;
                }
            }
             
            if(ok)
            {
                /* add to array */
                Code_O parentItem = null;
                Code_O lastItem = null;

                Subprogram sub = null;
                switch (code)
                {
                    case CodeOType_et.CodeO_call:
                    case CodeOType_et.CodeO_return:
                    case CodeOType_et.CodeO_sub:
                    case CodeOType_et.CodeO_endsub:
                        sub = subprograms.Find(x => x.seqNo == seqNo);
                        if(sub == null)
                        {

                            sub = new Subprogram(seqNo);
                            subprograms.Add(sub);
                        }
                        if(code == CodeOType_et.CodeO_sub)
                        {
                            actSubprogram = sub;
                            codeArray = sub.codeArray;
                        }
                        else if (code == CodeOType_et.CodeO_endsub)
                        {
                            /* add return at end of subprogram */
                            Code_O returnItem = new Code_O(code);
                            returnItem.orgIdx = seqNo;
                            returnItem.code = CodeOType_et.CodeO_return;
                            returnItem.seqNo = item.seqNo;
                            codeArray.Add( returnItem );


                            actSubprogram = null;
                            codeArray = baseCodeArray;
                        }
                        break;

                    default:
                        parentItem = code_Os.Find(x => x.seqNo == seqNo);
                        lastItem = parentItem;
                        if (lastItem != null)
                        {
                            while (lastItem.next != null) { lastItem = lastItem.next; }
                        }
                        break;

                }
                item.idx = codeArray.Count - 1;

                switch (code)
                {
                    case CodeOType_et.CodeO_if:
                    case CodeOType_et.CodeO_repeat:
                    case CodeOType_et.CodeO_do:
                        if (parentItem != null)
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code sequence error");
                        }
                        else
                        {
                            item.parent = item;
                            code_Os.Add(item);
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                        }
                        break;
                    case CodeOType_et.CodeO_while:
                        if (parentItem != null && parentItem.code == CodeOType_et.CodeO_do && parentItem.next == null)
                        {
                            /* do-while case */
                            parentItem.next = item;
                            item.code = CodeOType_et.CodeO_enddo;
                            item.parent = parentItem;
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                        }
                        else if (parentItem == null)
                        {
                            code_Os.Add(item);
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                            item.parent = item;
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code sequence error");
                        }
                        break;
                    case CodeOType_et.CodeO_endrepeat:                    
                        if (parentItem != null && parentItem.code == CodeOType_et.CodeO_repeat && parentItem.next == null)
                        {                            
                            parentItem.next = item;
                            item.parent = parentItem;
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);                            
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code sequence error");
                        }
                        break;
                    case CodeOType_et.CodeO_endwhile:
                        if (parentItem != null && parentItem.code == CodeOType_et.CodeO_while && parentItem.next == null)
                        {
                            parentItem.next = item;
                            item.parent = parentItem;
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code sequence error");
                        }
                        break;
                    case CodeOType_et.CodeO_elseif:
                    case CodeOType_et.CodeO_else:
                        if (lastItem != null && (lastItem.code == CodeOType_et.CodeO_if || lastItem.code == CodeOType_et.CodeO_elseif))
                        {
                            lastItem.next = item;
                            item.parent = parentItem;
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code sequence error");
                        }
                        break;
                    case CodeOType_et.CodeO_endif:
                        if (lastItem != null && (lastItem.code == CodeOType_et.CodeO_if || lastItem.code == CodeOType_et.CodeO_elseif || lastItem.code == CodeOType_et.CodeO_else))
                        {
                            lastItem.next = item;
                            item.parent = parentItem;
                            item.orgIdx = lineItm.orgIdx;
                            codeArray.Add(item);
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "O_code sequence error");
                        }
                        break;
                    case CodeOType_et.CodeO_call:
                        item.orgIdx = lineItm.orgIdx;
                        codeArray.Add(item);
                        item.sub = sub;
                        break;
                    case CodeOType_et.CodeO_sub:
                        sub.parsed = true;
                        item.orgIdx = lineItm.orgIdx;
                        //codeArray.Add(item);
                        item.sub = sub;
                        if (parsingSub == null)
                        {
                            parsingSub = sub;
                            math = sub.math;
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "Invalid subprogram definition");
                        }                     
                        break;
                    case CodeOType_et.CodeO_endsub:
                        item.orgIdx = lineItm.orgIdx;
                        //codeArray.Add(item);
                        item.sub = sub;
                        if (parsingSub == sub)
                        {
                            parsingSub = null;
                            math = baseMath;
                        }
                        else
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "Invalid subprogram definition");
                        }
                        break;
                    case CodeOType_et.CodeO_return:
                        item.orgIdx = lineItm.orgIdx;
                        codeArray.Add(item);
                        item.sub = sub;
                        if (parsingSub != sub)
                        {
                            ok = false;
                            PrintError(lineItm.orgIdx, "Invalid subprogram definition");
                        }
                        break;
                    default:
                        ok = false;
                        PrintError(lineItm.orgIdx, "Unknown O_code!");
                        break;
                }
            }

            if(ok == false)
            {
                PrintError(lineItm.orgIdx, "General error!");
            }

            return ok;
        }



        


    }
}
