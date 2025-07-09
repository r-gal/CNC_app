using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
//using System.Windows.Media.Media3D;
//using System.Windows.Media.TextFormatting;

namespace CNC3
{


    internal class PipelineUnit
    {
        List<MoveData> movesList; 
        

        SpeedUnit speedUnit;
        Executor executor;

        bool stopped = false; 

        private void InitTest()
        {
           /* MoveData moveData = new MoveData();

            moveData.startSpeed = 0;
            moveData.endSpeed = 0;

            moveData.maxSpeed = 25;
            moveData.maxEndSpeed = 0;
            moveData.maxAcceleration = 20;
            moveData.point1[0] = 0;
            moveData.point1[1] = 0;
            moveData.point1[2] = 20000;
            moveData.point1[3] = 0;
            moveData.seqNo = 0;
            moveData.orderCode = MoveData.orderCode_et.LINE;

            AddMove(moveData);


            moveData = new MoveData();

            moveData.startSpeed = 0;
            moveData.endSpeed = 0;

            moveData.maxSpeed = 25;
            moveData.maxEndSpeed = 0;
            moveData.maxAcceleration = 50;
            moveData.point1[0] = 0;
            moveData.point1[1] = 0;
            moveData.point1[2] = -20000;
            moveData.point1[3] = 0;
            moveData.seqNo = 1;
            moveData.orderCode = MoveData.orderCode_et.LINE;

            AddMove(moveData);


            moveData = new MoveData();

            moveData.startSpeed = 0;
            moveData.endSpeed = 0;

            moveData.maxSpeed = 10;
            moveData.maxEndSpeed = 0;
            moveData.maxAcceleration = 20;
            moveData.point1[0] = 0;
            moveData.point1[1] = 0;
            moveData.point1[2] = -20000;
            moveData.point1[3] = 0;
            moveData.seqNo = 2;
            moveData.orderCode = MoveData.orderCode_et.LINE;

            AddMove(moveData);*/
        }

        public PipelineUnit(Executor executor_)
        {
            movesList = new List<MoveData>();

            


            executor = executor_;            
            speedUnit = new SpeedUnit();
            InitTest();

            
        }



        public void AddMove(MoveData move)
        {
            
            movesList.Add(move);
            if (movesList.Count > 1)
            {
                int idx = movesList.Count - 1;

                double transferSpeed = speedUnit.CalcTransferSpeed(movesList[idx - 1], movesList[idx]);
                movesList[idx - 1].maxEndSpeed = transferSpeed;

            }
            else
            {
                movesList[0].maxEndSpeed = 0;
            }
        }

        public MoveData GetMove(bool stepMode)
        {
            if (movesList.Count == 0)
            {
                stopped = false;
                FillPipeline();                
            }

            if (movesList.Count > 0)
            {
                speedUnit.RecalcSpeeds(movesList, stepMode);
                MoveData move = movesList.First();
                movesList.Remove(move);
                if (stopped == false)
                {
                    FillPipeline();
                }
                return move;
            }
            else
            {
                return null;
            }            
        }

        private void FillPipeline()
        {
            bool cont = true;
            while(movesList.Count < Constants.PIPELINE_LENGTH && cont)
            {
                MoveData[] moveArray = executor.GetMove();
                if (moveArray != null)
                {
                    for (int i = 0; i < moveArray.Length; i++)
                    { 
                        speedUnit.CalcMaxSpeed(moveArray[i]);
                        AddMove(moveArray[i]);

                        if (moveArray[i].orderCode == MoveData.orderCode_et.PROBE)
                        {
                            cont = false;
                            stopped = true;
                            break;
                            
                        }
                        else if (moveArray[i].orderCode == MoveData.orderCode_et.PAUSE_TOOL_CHANGE)
                        {
                            cont = false;
                            stopped = true;
                            break;
                        }
                    }  
                }
                else
                {
                    break;
                }

            }
        }

        public bool PrepareRun(int startLine, bool simulation, string[] codeFile)
        {
            stopped = false;
            movesList.Clear();
            bool ok = executor.Start(startLine, simulation, codeFile);
            if(ok)
            {
                //FillPipeline();
            }
            return ok;
            
        }

        public void StopSimulation()
        {
            executor.StopSimulation();
        }
    }
}
