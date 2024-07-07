using System;
using System.IO.Ports;

namespace Assets.Scripts.Agents.Controls
{
	/// <summary>
	/// Class to control player with a trainingdevice.
	/// </summary>
	/// <todo>Implement</todo>
	class RPMProvider : IInputProvider
	{
		public int rpm;
		public byte address;
		public SerialPort port;

		public string portName = "COM3";

		public void OnEnable()
		{
			//BikeController.Initialize();
			BikeController.Run(portName);
		}

		public void Update()
		{
			BikeController.Update();
			rpm = BikeController.getRPM();
			address = BikeController.address;
			port = BikeController.port;
		}

		public override bool GetDirection()
		{
			throw new NotImplementedException();
		}

		public override float GetSpeedNormalized()
		{
			throw new NotImplementedException();
		}
	}
}
