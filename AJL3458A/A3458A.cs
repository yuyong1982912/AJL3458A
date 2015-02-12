using System;
using System.Collections.Generic;
using System.Text;
using Ivi.Visa.Interop;



namespace AJL3458A
{
	public class A3458A
	{
		/// <summary>
		/// 取得3458A的值，取5次后取平均值
		/// </summary>
		public static double Get()
        {
            StringBuilder strDataCommand = new StringBuilder();
            string instAddress = "GPIB0::22::INSTR";
            ResourceManagerClass oRm = new ResourceManagerClass();
            FormattedIO488Class oFio = new FormattedIO488Class();

            //Open session for instrument.
            oFio.IO = (IMessage)oRm.Open(instAddress, AccessMode.NO_LOCK, 2000, "");
            oFio.IO.Timeout = 10000;   //set timeout to 10 seconds

            //Reset instrument
            oFio.WriteString("RESET", true);

            //Query Idendity string and report.
            oFio.WriteString("END ALWAYS", true);   //Set endline termination so you can retrieve data from instrument            

            //Turn off memory, take data directly over bus
            oFio.WriteString("MEM OFF", true);

            // Specify DATA FORMAT
            oFio.WriteString("MFORMAT DINT", true);
            oFio.WriteString("OFORMAT DINT", true);

            //Configure the instrument            
            oFio.WriteString("DCV 10", true);
            oFio.WriteString("TRIG AUTO", true);
            oFio.WriteString("NRDGS 5", true);  //Set number of readings per trigger to desired value
            oFio.WriteString("NPLC 1", true); //Set desired number of NPLCs, increase for greater accuracy
            oFio.WriteString("ISCALE?", true);  //Get scale value so you can convert DINT data

            double scale = 0;
            string scalestring;

            scalestring = oFio.ReadString();
            scale = double.Parse(scalestring);//Convert string result to double

            oFio.WriteString("END ON", true);  //Have to use END ON with multiple readings
            oFio.WriteString("TARM SGL", true); //Triggers the instrument 1 time then becomes hold
            
            byte[] rawdata = new byte[22];  //size should be at least nrdgs*4 +2
            double[] scaleddata = new double[5];   //size should be at least nrdgs

            //Read the DINT data
            rawdata = oFio.IO.Read(22);    //nrdgs*4 + 2
            byte[] tempBuffer = new byte[4];

            //Convert the DINT reading data and scale it with ISCALE then output to screen
            for (int i = 0; i < 5; i++)
            {
                tempBuffer[3] = rawdata[i * 4];
                tempBuffer[2] = rawdata[i * 4 + 1];
                tempBuffer[1] = rawdata[i * 4 + 2];
                tempBuffer[0] = rawdata[i * 4 + 3];
                scaleddata[i] = System.BitConverter.ToInt32(tempBuffer, 0) * scale;                
            }
			
            double dResult=0;
            for (int i = 0; i < scaleddata.Length; i++) {
            	dResult=dResult+scaleddata[i];
            }
            oFio.IO.Close();
            return dResult/5; 
        }
	}
}