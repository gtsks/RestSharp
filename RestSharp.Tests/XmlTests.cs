﻿#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Xml.Linq;
using RestSharp.Deserializers;
using Xunit;

namespace RestSharp.Tests
{
	public class XmlTests
	{
		private const string GuidString = "AC1FC4BC-087A-4242-B8EE-C53EBE9887A5";

		[Fact]
		public void Can_Deserialize_Empty_Elements_to_Nullable_Values() {
			var doc = CreateXmlWithNullValues();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<NullableValues>(new RestResponse { Content = doc });

			Assert.Null(output.Id);
			Assert.Null(output.StartDate);
			Assert.Null(output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Elements_to_Nullable_Values() {
			var doc = CreateXmlWithoutEmptyValues();

			var xml = new XmlDeserializer();
			var output = xml.Deserialize<NullableValues>(new RestResponse { Content = doc });

			Assert.NotNull(output.Id);
			Assert.NotNull(output.StartDate);
			Assert.NotNull(output.UniqueId);

			Assert.Equal(123, output.Id);
			Assert.Equal(new DateTime(2010, 2, 21, 9, 35, 00), output.StartDate);
			Assert.Equal(new Guid(GuidString), output.UniqueId);
		}

		[Fact]
		public void Can_Deserialize_Custom_Formatted_Date() {
			var format = "dd yyyy MMM, hh:mm ss tt zzz";
			var date = new DateTime(2010, 2, 8, 11, 11, 11);

			var doc = new XDocument();

			var root = new XElement("Person");
			root.Add(new XElement("StartDate", date.ToString(format)));

			doc.Add(root);

			var xml = new XmlDeserializer();
			xml.DateFormat = format;

			var response = new RestResponse { Content = doc.ToString() };
			var output = xml.Deserialize<PersonForXml>(response);

			Assert.Equal(date, output.StartDate);
		}

		[Fact]
		public void Can_Deserialize_Elements_On_Default_Root() {
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Can_Deserialize_Attributes_On_Default_Root() {
			var doc = CreateAttributesXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);
		}

		[Fact]
		public void Ignore_Protected_Property_That_Exists_In_Data() {
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Null(p.IgnoreProxy);
		}

		[Fact]
		public void Ignore_ReadOnly_Property_That_Exists_In_Data() {
			var doc = CreateElementsXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Null(p.ReadOnlyProxy);
		}

		[Fact]
		public void Can_Deserialize_Names_With_Underscores_On_Default_Root() {
			var doc = CreateUnderscoresXml();
			var response = new RestResponse { Content = doc };

			var d = new XmlDeserializer();
			var p = d.Deserialize<PersonForXml>(response);

			Assert.Equal("John Sheehan", p.Name);
			Assert.Equal(new DateTime(2009, 9, 25, 0, 6, 1), p.StartDate);
			Assert.Equal(28, p.Age);
			Assert.Equal(long.MaxValue, p.BigNumber);
			Assert.Equal(99.9999m, p.Percent);
			Assert.Equal(false, p.IsCool);
			Assert.Equal(new Guid(GuidString), p.UniqueId);

			Assert.NotNull(p.Friends);
			Assert.Equal(10, p.Friends.Count);

			Assert.NotNull(p.BestFriend);
			Assert.Equal("The Fonz", p.BestFriend.Name);
			Assert.Equal(1952, p.BestFriend.Since);

			Assert.NotNull(p.Foes);
			Assert.Equal(5, p.Foes.Count);
			Assert.Equal("Yankees", p.Foes.Team);
		}

		[Fact]
		public void Can_Deserialize_Eventful_Xml() {
			var xmlpath = Environment.CurrentDirectory + @"\SampleData\eventful.xml";
			var doc = XDocument.Load(xmlpath);
			var response = new RestResponse { Content = doc.ToString() };

			var d = new XmlDeserializer();
			var output = d.Deserialize<SampleClasses.VenueSearch>(response);

			Assert.NotEmpty(output.venues);
			Assert.Equal(3, output.venues.Count);
			Assert.Equal("Tivoli", output.venues[0].name);
			Assert.Equal("http://eventful.com/brisbane/venues/tivoli-/V0-001-002169294-8", output.venues[1].url);
			Assert.Equal("V0-001-000266914-3", output.venues[2].id);
		}

		private static string CreateUnderscoresXml() {
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XElement("Name", "John Sheehan"));
			root.Add(new XElement("Start_Date", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute("Age", 28));
			root.Add(new XElement("Percent", 99.9999m));
			root.Add(new XElement("Big_Number", long.MaxValue));
			root.Add(new XAttribute("Is_Cool", false));
			root.Add(new XElement("Ignore", "dummy"));
			root.Add(new XAttribute("Read_Only", "dummy"));
			root.Add(new XElement("Unique_Id", new Guid(GuidString)));

			root.Add(new XElement("Best_Friend",
						new XElement("Name", "The Fonz"),
						new XAttribute("Since", 1952)
					));

			var friends = new XElement("Friends");
			for (int i = 0; i < 10; i++) {
				friends.Add(new XElement("Friend",
								new XElement("Name", "Friend" + i),
								new XAttribute("Since", DateTime.Now.Year - i)
							));
			}
			root.Add(friends);

			var foes = new XElement("Foes");
			foes.Add(new XAttribute("Team", "Yankees"));
			for (int i = 0; i < 5; i++) {
				foes.Add(new XElement("Foe", new XElement("Nickname", "Foe" + i)));
			}
			root.Add(foes);

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateElementsXml() {
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XElement("Name", "John Sheehan"));
			root.Add(new XElement("StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XElement("Age", 28));
			root.Add(new XElement("Percent", 99.9999m));
			root.Add(new XElement("BigNumber", long.MaxValue));
			root.Add(new XElement("IsCool", false));
			root.Add(new XElement("Ignore", "dummy"));
			root.Add(new XElement("ReadOnly", "dummy"));
			root.Add(new XElement("UniqueId", new Guid(GuidString)));

			root.Add(new XElement("BestFriend",
						new XElement("Name", "The Fonz"),
						new XElement("Since", 1952)
					));

			var friends = new XElement("Friends");
			for (int i = 0; i < 10; i++) {
				friends.Add(new XElement("Friend",
								new XElement("Name", "Friend" + i),
								new XElement("Since", DateTime.Now.Year - i)
							));
			}
			root.Add(friends);

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateAttributesXml() {
			var doc = new XDocument();
			var root = new XElement("Person");
			root.Add(new XAttribute("Name", "John Sheehan"));
			root.Add(new XAttribute("StartDate", new DateTime(2009, 9, 25, 0, 6, 1)));
			root.Add(new XAttribute("Age", 28));
			root.Add(new XAttribute("Percent", 99.9999m));
			root.Add(new XAttribute("BigNumber", long.MaxValue));
			root.Add(new XAttribute("IsCool", false));
			root.Add(new XAttribute("Ignore", "dummy"));
			root.Add(new XAttribute("ReadOnly", "dummy"));
			root.Add(new XAttribute("UniqueId", new Guid(GuidString)));

			root.Add(new XElement("BestFriend",
						new XAttribute("Name", "The Fonz"),
						new XAttribute("Since", 1952)
					));

			doc.Add(root);
			return doc.ToString();
		}

		private static string CreateXmlWithNullValues() {
			var doc = new XDocument();
			var root = new XElement("NullableValues");

			root.Add(new XElement("Id", null),
					 new XElement("StartDate", null),
					 new XElement("UniqueId", null)
				);

			doc.Add(root);

			return doc.ToString();
		}

		private static string CreateXmlWithoutEmptyValues() {
			var doc = new XDocument();
			var root = new XElement("NullableValues");

			root.Add(new XElement("Id", 123),
					 new XElement("StartDate", new DateTime(2010, 2, 21, 9, 35, 00).ToString()),
					 new XElement("UniqueId", new Guid(GuidString))
					 );

			doc.Add(root);

			return doc.ToString();
		}
	}
}
