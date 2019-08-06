﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace MongoDB.DriverUnitTests.CommandResults
{
    [TestFixture]
    public class GetLastErrorResultTests
    {
        private MongoServer _server;
        private MongoDatabase _database;
        private MongoCollection<BsonDocument> _collection;

        [OneTimeSetUp]
        public void Setup()
        {
            _server = Configuration.TestServer;
            _database = Configuration.TestDatabase;
            _collection = Configuration.TestCollection;
        }

        [Test]
        public void TestInsert()
        {
            using (_database.RequestStart())
            {
                _collection.Insert(new BsonDocument());
#pragma warning disable 618
                var result = _server.GetLastError();
#pragma warning restore
                Assert.IsFalse(result.HasLastErrorMessage);
                Assert.IsFalse(result.UpdatedExisting);
                // server 3.2 has changed the GLE results for "n" on insert.
                Assert.That(result.DocumentsAffected, Is.EqualTo(0).Or.EqualTo(1));
            }
        }

        [Test]
        public void TestUpdate()
        {
            using (_database.RequestStart())
            {
                var id = ObjectId.GenerateNewId();
                var document = new BsonDocument
                {
                    { "_id", id },
                    { "x", 1 }
                };
                _collection.Insert(document);

                var query = Query.EQ("_id", id);
                var update = Update.Inc("x", 1);
                _collection.Update(query, update);
#pragma warning disable 618
                var result = _server.GetLastError();
#pragma warning restore
                Assert.IsFalse(result.HasLastErrorMessage);
                Assert.IsTrue(result.UpdatedExisting);
                Assert.AreEqual(1, result.DocumentsAffected);
            }
        }
    }
}
