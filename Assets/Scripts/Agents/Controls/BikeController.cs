using System;
using System.IO.Ports;
using System.Threading;

public class BikeController
{
	public static SerialPort port = null;
	static string portName = "COM3";
	public static byte address = 0;
	static int rpm = 0;

	static void RequestAddress()
	{
		byte[] buffer = { 0x11 };
		port.Write(buffer, 0, buffer.Length);
		Console.WriteLine("Sent data.");
		Thread.Sleep(100);
	}


	static void RequestRPM()
	{
		byte[] buffer = { 0x40, address };
		port.Write(buffer, 0, buffer.Length);
		Console.WriteLine("Sent data.");
		//Thread.Sleep(100);
	}

	static byte ReadAddress()
	{
		byte address = 0;
		Console.WriteLine("Reading data.");
		for (int i = 0; i < 100; ++i)
		{
			try
			{
				int b = port.ReadByte();
				if (i == 1) address = (byte)b;
			}
			catch (TimeoutException)
			{
				break;
			}
		}
		return address;
	}

	static int ReadRPM()
	{
		int rpm = 0;
		for (int i = 0; i < 100; ++i)
		{
			try
			{
				int b = port.ReadByte();
				if (i == 6) rpm = b;
			}
			catch (TimeoutException)
			{
				UnityEngine.Debug.Log("Got Timeout!");
				break;
			}
		}
		return rpm;
	}

	public static void Run(string portName)
	{
		port = new SerialPort(portName);
		port.BaudRate = 9600;
		port.Parity = Parity.None;
		port.StopBits = StopBits.One;
		port.DataBits = 8;
		port.ReadTimeout = 50; // 1 ms bis timeout

		port.Handshake = Handshake.None;
		port.Open(); // could take some time

		RequestAddress();
		address = ReadAddress();
		//Console.WriteLine(address);
	}

	public static void Update()
	{	
		RequestRPM();
		rpm = ReadRPM();
		UnityEngine.Debug.Log("RPM: " + rpm);
	}

	public static int getRPM()
	{
		return rpm;
	}

	public static void Initialize(string comPort = "COM3")
	{
		portName = comPort;
		//Thread thread = new Thread(Run);
		//thread.Start();
	}
}
