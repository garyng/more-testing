using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MoreTesting.Tests
{
	[TestFixture]
	public class MoreTestingTests
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
