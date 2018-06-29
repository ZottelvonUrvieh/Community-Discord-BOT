using System;
using CommunityBot.Features.RoleAssignment;
using NUnit.Framework;

namespace CommunityBot.Tests.FeatureTests
{
    public class RoleByPhraseTests
    {
        private static RoleByPhraseSettings GetFilledSettings()
        {
            return new RoleByPhraseSettings
            {
                Phrases =
                {
                    "A",
                    "B"
                },
                RolesIds =
                {
                    111,
                    222,
                    333
                },
                Relations =
                {
                    new RoleByPhraseRelation
                    {
                        PhraseIndex = 0,
                        RoleIdIndex = 0
                    },
                    new RoleByPhraseRelation
                    {
                        PhraseIndex = 1,
                        RoleIdIndex = 0
                    },
                    new RoleByPhraseRelation
                    {
                        PhraseIndex = 0,
                        RoleIdIndex = 1
                    },
                    new RoleByPhraseRelation
                    {
                        PhraseIndex = 2,
                        RoleIdIndex = 2
                    }
                }
            };
        }

        [Test]
        public void Rbp_RemoveRelationTest()
        {
            const int expectedRelCount = 3;
            var rbps = GetFilledSettings();

            rbps.RemoveRelation(0, 0);
            var actual = rbps.Relations.Count;

            Assert.AreEqual(expectedRelCount, actual);
        }

        [Test]
        public void Rbp_RemoveRelationTest_ArgEx()
        {
            var rbps = GetFilledSettings();

            const int validIndex = 0;
            const int invalidIndex = 999;

            RemoveRelationArgExceptionTest(rbps, validIndex, invalidIndex);
            RemoveRelationArgExceptionTest(rbps, invalidIndex, validIndex);
            RemoveRelationArgExceptionTest(rbps, invalidIndex, invalidIndex);
            
            Assert.Pass("Passed all Exception tests.");
        }

        private static void RemoveRelationArgExceptionTest(RoleByPhraseSettings rbps, int pId, int rId)
        {
            try
            {
                rbps.RemoveRelation(pId, rId);
            }
            catch (ArgumentException)
            {
                return;
            }
            Assert.Fail($"Did not throw ArgumentException for {pId} : {rId}.");
        }

        [Test]
        public void Rbp_RemoveRelationTest_RelNotFoundEx()
        {
            var rbps = GetFilledSettings();

            try
            {
                rbps.RemoveRelation(1,1);
            }
            catch (RelationNotFoundException)
            {
                Assert.Pass();
            }

            Assert.Fail("RemoveRelation did not throw RelationNotFoundException.");
        }

        [Test]
        public void Rbp_AddRelationTest()
        {
            const int expectedRelCount = 5;
            var rbps = GetFilledSettings();

            rbps.CreateRelation(0, 2);
            var acutal = rbps.Relations.Count;

            Assert.AreEqual(expectedRelCount, acutal);
        }

        [Test]
        public void Rbp_AddRelationTest_ArgEx()
        {
            var rbps = GetFilledSettings();

            const int validIndex = 0;
            const int invalidIndex = 999;

            AddRelationArgExceptionTest(rbps, validIndex, invalidIndex);
            AddRelationArgExceptionTest(rbps, invalidIndex, validIndex);
            AddRelationArgExceptionTest(rbps, invalidIndex, invalidIndex);

            Assert.Pass("Passed all Exception tests.");
        }

        private static void AddRelationArgExceptionTest(RoleByPhraseSettings rbps, int pId, int rId)
        {
            try
            {
                rbps.CreateRelation(pId, rId);
            }
            catch (ArgumentException)
            {
                return;
            }
            Assert.Fail($"Did not throw ArgumentException for {pId} : {rId}.");
        }

        [Test]
        public void Rbp_AddRelationTest_RelExists()
        {
            var rbps = GetFilledSettings();

            try
            {
                rbps.CreateRelation(1, 0);
            }
            catch (RelationAlreadyExistsException)
            {
                Assert.Pass();
            }

            Assert.Fail("CreateRelation did not throw RelationAlreadyExistsException.");
        }

        [Test]
        public void Rbp_AddPhraseTest()
        {
            const int expected = 3;
            var rbps = GetFilledSettings();

            rbps.AddPhrase("CC");
            var actual = rbps.Phrases.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Rbp_AddPhraseTest_InvalidPhraseEx()
        {
            var rbps = GetFilledSettings();

            try
            {
                rbps.AddPhrase(string.Empty);
            }
            catch (InvalidPhraseException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void Rbp_AddPhraseTest_InvalidPhraseEx_TooLong()
        {
            var rbps = GetFilledSettings();

            try
            {
                rbps.AddPhrase(new string('A', Constants.MaxMessageLength));
            }
            catch (InvalidPhraseException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void Rbp_RemovePhraseTest()
        {
            const int expected = 1;
            var rbps = GetFilledSettings();

            rbps.RemovePhraseByIndex(1);
            var actual = rbps.Phrases.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Rbp_AddRoleTest()
        {
            const int expected = 4;
            var rbps = GetFilledSettings();

            rbps.AddRole(123);
            var actual = rbps.RolesIds.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Rbp_AddRoleTest_AlreadyAddedEx()
        {
            var rbps = GetFilledSettings();

            try
            {
                rbps.AddRole(111);
            }
            catch (RoleIdAlreadyAddedException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void Rbp_RemoveRoleTest()
        {
            const int expected = 2;
            var rbps = GetFilledSettings();

            rbps.RemoveRoleIdByIndex(0);
            var actual = rbps.RolesIds.Count;

            Assert.AreEqual(expected, actual);
        }
    }
}