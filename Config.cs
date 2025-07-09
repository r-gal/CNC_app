using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using static CNC3.ConfigData;

namespace CNC3
{

    public class MacroData
    {
        public string name;
        public string path;
        public bool needConfirm;
    };
    public class ConfigData
    {
        public enum LIM_MODE_et
        {
            NONE,
            ONE,
            DUAL           
        };

        public enum LIM_TYPE_et
        {
            NO,
            NC
        };


        public enum BASE_TYPE_et
        {
            NONE,
            LIMTERS,
            SEPARATE
        };

        public enum INPUT_MODE_et
        {
            NO,
            NC
        };


        public IPAddress ip;
        public int port;
        public bool autoConnect;

        public INPUT_MODE_et eStopMode;
        public INPUT_MODE_et probeMode;
        public double minSpeed;
        public double autoBaseSpeed;
        public int maxSpindleSpeed;

        public double baseX;
        public double baseY;
        public double baseZ;
        public double probeSpeed1;
        public double probeSpeed2;
        public double probeLength;

        public double[] maxSpeed; /* [mm/s] */
        public double[] maxAcceleration; /* [mm/s2] */
        public int[] length;
        public bool[] ena;
        public bool[] dir;
        public double[] scale;
        public LIM_MODE_et[] limMode;
        public LIM_TYPE_et[] limType;
        public BASE_TYPE_et[] baseType;

        public bool autoTLO;
        public bool zeroMachineAutobase;

        public int[] zeroOffset;


        public MacroData[] macroConfig;


        public ConfigData()
        {
            maxSpeed = new double[Constants.NO_OF_AXES];
            maxAcceleration = new double[Constants.NO_OF_AXES];
            length = new int[Constants.NO_OF_AXES] ;
            ena = new bool[Constants.NO_OF_AXES];
            dir = new bool[Constants.NO_OF_AXES]; ;
            scale = new double[Constants.NO_OF_AXES] ;
            limMode = new LIM_MODE_et[Constants.NO_OF_AXES];
            limType = new LIM_TYPE_et[Constants.NO_OF_AXES];
            baseType = new BASE_TYPE_et[Constants.NO_OF_AXES];
            zeroOffset = new int[Constants.NO_OF_AXES];


            for (int i = 0; i < Constants.NO_OF_AXES; i++)
            {
                maxAcceleration[i] = 30;
                maxSpeed[i] = 50;
                length[i] = 100;
                ena[i] = false;
                dir[i] = false;
                scale[i] = 10;
                limMode[i] = LIM_MODE_et.NONE;
                limType[i] = LIM_TYPE_et.NO;
                baseType[i] = BASE_TYPE_et.NONE;
                zeroOffset[i] = 0;
            }

            macroConfig = new MacroData[Constants.NO_OF_MACROS];

            for (int i = 0; i < Constants.NO_OF_MACROS; i++)
            {
                macroConfig[i] = new MacroData();
                macroConfig[i].name = "MACRO" + (i + 1).ToString();
                macroConfig[i].path = "";
                macroConfig[i].needConfirm = false;
            }

            ip = IPAddress.Parse("192.168.55.40");
            port = 4010;
            autoConnect = true;

            eStopMode = INPUT_MODE_et.NO;
            probeMode = INPUT_MODE_et.NO;
            minSpeed = 0.5;
            autoBaseSpeed = 2;
            maxSpindleSpeed = 18000;

            autoTLO = false;
            zeroMachineAutobase = true;

            baseX = 0;
            baseY = 0;
            baseZ = 0;
            probeSpeed1 = 0;
            probeSpeed2 = 0;
            probeLength = 0;
        }


    };

    internal class Config
    {
        public static ConfigData configData = new ConfigData();

        public delegate void CallbackConfigChanged();
        public static event CallbackConfigChanged ConfigChangedCallback;
        public static void SaveConfig()
        {
            XmlDocument config = new XmlDocument();

            XmlElement rootElement = (XmlElement)config.AppendChild(config.CreateElement("ROOT"));

            XmlElement conConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("Connection"));

            conConfig.AppendChild(config.CreateElement("IP")).InnerText = configData.ip.ToString();
            conConfig.AppendChild(config.CreateElement("PORT")).InnerText = configData.port.ToString();
            conConfig.AppendChild(config.CreateElement("AUTOCONNECT")).InnerText = configData.autoConnect.ToString();

            XmlElement miscConfig = (XmlElement)rootElement.AppendChild(config.CreateElement("Misc"));

            miscConfig.AppendChild(config.CreateElement("ESTOPMODE")).InnerText = configData.eStopMode.ToString();
            miscConfig.AppendChild(config.CreateElement("PROBEMODE")).InnerText = configData.probeMode.ToString();
            miscConfig.AppendChild(config.CreateElement("MIN_SPEED")).InnerText = configData.minSpeed.ToString();
            miscConfig.AppendChild(config.CreateElement("AUTOBASE_SPEED")).InnerText = configData.autoBaseSpeed.ToString();
            miscConfig.AppendChild(config.CreateElement("SPINDLE_MAX_SPEED")).InnerText = configData.maxSpindleSpeed.ToString();
            miscConfig.AppendChild(config.CreateElement("AUTO_TLO")).InnerText = configData.autoTLO.ToString();
            miscConfig.AppendChild(config.CreateElement("ZERO_HOME_AFTER_AUTOBASE")).InnerText = configData.zeroMachineAutobase.ToString();


            miscConfig.AppendChild(config.CreateElement("BASE_X")).InnerText = configData.baseX.ToString();
            miscConfig.AppendChild(config.CreateElement("BASE_Y")).InnerText = configData.baseY.ToString();
            miscConfig.AppendChild(config.CreateElement("BASE_Z")).InnerText = configData.baseZ.ToString();
            miscConfig.AppendChild(config.CreateElement("PROBE_SPEED_1")).InnerText = configData.probeSpeed1.ToString();
            miscConfig.AppendChild(config.CreateElement("PROBE_SPEED_2")).InnerText = configData.probeSpeed2.ToString();
            miscConfig.AppendChild(config.CreateElement("PROBE_LENGTH")).InnerText = configData.probeLength.ToString();

            for (int axe = 0;axe< Constants.NO_OF_AXES; axe++)
            {
                string name = "";
                switch(axe)
                {
                    case 0: name = "AXE_X"; break;
                    case 1: name = "AXE_Y"; break;
                    case 2: name = "AXE_Z"; break;
                    case 3: name = "AXE_A"; break;
                    default: name = "AXE_UNN"; break;
                }

                XmlElement el = (XmlElement)rootElement.AppendChild(config.CreateElement(name));
                //el.SetAttribute("Bar", "some & value");
                el.AppendChild(config.CreateElement("Enable")).InnerText = configData.ena[axe].ToString();
                el.AppendChild(config.CreateElement("Scale")).InnerText = configData.scale[axe].ToString();
                el.AppendChild(config.CreateElement("MaxSpeed")).InnerText = configData.maxSpeed[axe].ToString();
                el.AppendChild(config.CreateElement("MaxAcceleration")).InnerText = configData.maxAcceleration[axe].ToString();
                el.AppendChild(config.CreateElement("Length")).InnerText = configData.length[axe].ToString();
                el.AppendChild(config.CreateElement("LimMode")).InnerText = configData.limMode[axe].ToString();
                el.AppendChild(config.CreateElement("LimType")).InnerText = configData.limType[axe].ToString();
                el.AppendChild(config.CreateElement("BaseType")).InnerText = configData.baseType[axe].ToString();
                el.AppendChild(config.CreateElement("Direction")).InnerText = configData.dir[axe].ToString();
                el.AppendChild(config.CreateElement("ZeroOffset")).InnerText = configData.zeroOffset[axe].ToString();


            }

            for (int i = 0; i < Constants.NO_OF_MACROS; i++)
            {
                XmlElement el = (XmlElement)rootElement.AppendChild(config.CreateElement("MACRO_"+(i+1).ToString()));
                el.AppendChild(config.CreateElement("NAME")).InnerText = configData.macroConfig[i].name;
                el.AppendChild(config.CreateElement("PATH")).InnerText = configData.macroConfig[i].path;
                el.AppendChild(config.CreateElement("CONFIRM")).InnerText = configData.macroConfig[i].needConfirm.ToString();
            }

            config.Save("config.xml");

            ConfigChangedCallback();
        }


        public static  void SaveConnectionConfig(ConfigData configData_)
        {
            configData.autoConnect = configData_.autoConnect;
            configData.port = configData_.port;
            configData.ip = configData_.ip;

            SaveConfig();
        }

        public static void SaveMiscConfig(ConfigData configData_)
        {
            configData.eStopMode = configData_.eStopMode;
            configData.probeMode = configData_.probeMode;
            configData.minSpeed = configData_.minSpeed;
            configData.autoBaseSpeed = configData_.autoBaseSpeed;
            configData.maxSpindleSpeed = configData_.maxSpindleSpeed;
            configData.baseX = configData_.baseX;
            configData.baseY = configData_.baseY;
            configData.baseZ = configData_.baseZ;
            configData.probeSpeed1 = configData_.probeSpeed1;
            configData.probeSpeed2 = configData_.probeSpeed2;
            configData.probeLength = configData_.probeLength;
            configData.autoTLO = configData_.autoTLO;
            configData.zeroMachineAutobase = configData_.zeroMachineAutobase;


            SaveConfig();
        }

        public static void SaveAxeConfig(ConfigData configData_,int axe)
        {
            configData.maxAcceleration[axe] = configData_.maxAcceleration[axe];
            configData.maxSpeed[axe] = configData_.maxSpeed[axe];
            configData.length[axe] = configData_.length[axe];
            configData.ena[axe] = configData_.ena[axe];
            configData.dir[axe] = configData_.dir[axe];
            configData.scale[axe] = configData_.scale[axe];
            configData.limMode[axe] = configData_.limMode[axe];
            configData.limType[axe] = configData_.limType[axe];
            configData.baseType[axe] = configData_.baseType[axe];
            configData.zeroOffset[axe] = configData_.zeroOffset[axe];
            SaveConfig();
        }

        public static void SaveMacroConfig(MacroData configData_,int macroIdx)
        {
            configData.macroConfig[macroIdx].name = configData_.name;
            configData.macroConfig[macroIdx].path = configData_.path;
            configData.macroConfig[macroIdx].needConfirm = configData_.needConfirm;

            SaveConfig();
        }

        public static void LoadConfig()
        {
            XmlDocument config = new XmlDocument();

            bool ok = false;

            try
            {
                config.Load("config.xml");
                ok = true;
            }
            catch
            {
                MessageBox.Show("Missing configuration file, use default settings", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (ok)
            {
                XmlNode node;

                try
                {
                    node = config.SelectSingleNode("ROOT/Connection/IP");
                    configData.ip = IPAddress.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Connection/PORT");
                    configData.port = int.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Connection/AUTOCONNECT");
                    configData.autoConnect = bool.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/ESTOPMODE");
                    configData.eStopMode = (ConfigData.INPUT_MODE_et)Enum.Parse(typeof(ConfigData.INPUT_MODE_et), node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/PROBEMODE");
                    configData.probeMode = (ConfigData.INPUT_MODE_et)Enum.Parse(typeof(ConfigData.INPUT_MODE_et), node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/MIN_SPEED");
                    configData.minSpeed = double.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/AUTOBASE_SPEED");
                    configData.autoBaseSpeed = double.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/SPINDLE_MAX_SPEED");
                    configData.maxSpindleSpeed = int.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/AUTO_TLO");
                    configData.autoTLO = bool.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/ZERO_HOME_AFTER_AUTOBASE");
                    configData.zeroMachineAutobase = bool.Parse(node.InnerText);
                }
                catch { ok = false; }



                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/BASE_X");
                    configData.baseX = double.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/BASE_Y");
                    configData.baseY = double.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/BASE_Z");
                    configData.baseZ = double.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/PROBE_SPEED_1");
                    configData.probeSpeed1 = double.Parse(node.InnerText);
                }
                catch { ok = false; }
                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/PROBE_SPEED_2");
                    configData.probeSpeed2 = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                try
                {
                    node = config.SelectSingleNode("ROOT/Misc/PROBE_LENGTH");
                    configData.probeLength = double.Parse(node.InnerText);
                }
                catch { ok = false; }

                for (int axe = 0; axe < Constants.NO_OF_AXES; axe++)
                {
                    string path = "";
                    switch (axe)
                    {
                        case 0: path = "ROOT/AXE_X/"; break;
                        case 1: path = "ROOT/AXE_Y/"; break;
                        case 2: path = "ROOT/AXE_Z/"; break;
                        case 3: path = "ROOT/AXE_A/"; break;
                    }
                    try
                    {
                        node = config.SelectSingleNode(path + "Enable");
                        configData.ena[axe] = bool.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "Scale"); ;
                        configData.scale[axe] = double.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "MaxSpeed");
                        configData.maxSpeed[axe] = double.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "MaxAcceleration");
                        configData.maxAcceleration[axe] = double.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "Length");
                        configData.length[axe] = int.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "LimMode");
                        configData.limMode[axe] = (ConfigData.LIM_MODE_et)Enum.Parse(typeof(ConfigData.LIM_MODE_et), node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "LimType");
                        configData.limType[axe] = (ConfigData.LIM_TYPE_et)Enum.Parse(typeof(ConfigData.LIM_TYPE_et), node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "BaseType");
                        configData.baseType[axe] = (ConfigData.BASE_TYPE_et)Enum.Parse(typeof(ConfigData.BASE_TYPE_et), node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "Direction");
                        configData.dir[axe] = bool.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                    try
                    {
                        node = config.SelectSingleNode(path + "ZeroOffset");
                        configData.zeroOffset[axe] = int.Parse(node.InnerText);
                    }
                    catch { ok = false; }

                    if (ok == false)
                    {
                        MessageBox.Show("Missing data in configuration file, use default settings", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

                for (int i = 0; i < Constants.NO_OF_MACROS; i++)
                {
                    string path = "ROOT/MACRO_" + (i + 1).ToString() + "/";

                    try
                    {
                        node = config.SelectSingleNode(path + "NAME");
                        configData.macroConfig[i].name = node.InnerText;
                    }
                    catch { ok = false; }

                    try
                    {
                        node = config.SelectSingleNode(path + "PATH");
                        configData.macroConfig[i].path = node.InnerText;
                    }
                    catch { ok = false; }

                    try
                    {
                        node = config.SelectSingleNode(path + "CONFIRM");
                        configData.macroConfig[i].needConfirm = bool.Parse(node.InnerText);
                    }
                    catch { ok = false; }
                }
            }

        }
    }
}
