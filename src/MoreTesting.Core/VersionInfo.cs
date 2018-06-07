using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MoreTesting.Core
{
	public class VersionInfo
	{
		public string Title => "Fancy new feature!";

		public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
		public string FileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
		public string ProductVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

		[JsonIgnore]
		public string Json => JsonConvert.SerializeObject(this, Formatting.Indented);


	}
}