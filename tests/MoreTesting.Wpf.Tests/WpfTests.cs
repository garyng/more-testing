using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MoreTesting.Wpf.Tests
{
	[TestFixture]
	public class WpfTests
	{
		[Test]
		public void Should_Pass_This()
		{
			// Arrange


			// Act


			// Assert
			Assert.Pass();
		}

		[Test]
		public void Should_RandomlyFail()
		{
			// Arrange


			// Act


			// Assert
			if (new Random().Next(100) > 50)
			{
				Assert.Pass();
			}
			else
			{
				Assert.Fail();
			}
		}

		[Test]
		[Category("IntegrationTests")]
		public void Should_BeSkipped()
		{
			// Arrange


			// Act


			// Assert
			Assert.Pass();
		}
	}

}
