using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MoreTesting.Core
{
	public class Testing
	{
		public string Title => "More Testing";
		
		[JsonIgnore]
		public string Json => JsonConvert.SerializeObject(this, Formatting.Indented);


	}
}