using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreTesting.Core;

namespace MoreTesting.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var versionInfo = new VersionInfo();
			System.Console.WriteLine(versionInfo.Json);
			System.Console.ReadKey();
		}
	}
}
