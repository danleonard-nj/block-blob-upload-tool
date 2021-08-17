using System;
using System.Collections.Generic;
using System.Text;

namespace BlobUploadTool.Utilities
{
		public static class ConsoleWriter
		{
				public static void WriteBlue(string message)
				{
						WriteConsole(message, ConsoleColor.Blue);
				}

				public static void WriteRed(string message)
				{
						WriteConsole(message, ConsoleColor.Red);
				}

				public static void WriteWhite(string message)
				{
						WriteConsole(message, ConsoleColor.White);
				}

				public static void WriteGreen(string message)
				{
						WriteConsole(message, ConsoleColor.Green);
				}

				public static void WriteConsole(string message, ConsoleColor consoleColor)
				{
						Console.ForegroundColor = consoleColor;
						Console.WriteLine(message);
						Console.ResetColor();
				}
		}
}
