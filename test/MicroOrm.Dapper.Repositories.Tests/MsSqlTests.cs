﻿using System.Linq;
using System.Threading.Tasks;
using MicroOrm.Dapper.Repositories.Tests.Classes;
using MicroOrm.Dapper.Repositories.Tests.DatabaseFixture;
using MicroOrm.Dapper.Repositories.Tests.DbContexts;
using Xunit;


namespace MicroOrm.Dapper.Repositories.Tests
{

    public class MsSqlTests : IClassFixture<MsSqlDatabaseFixture>
    {
        private readonly MsSqlDatabaseFixture _sqlDatabaseFixture;


        public MsSqlTests(MsSqlDatabaseFixture msSqlDatabaseFixture)
        {
            _sqlDatabaseFixture = msSqlDatabaseFixture;
        }


        [Fact]
        public void Find()
        {
            var user = _sqlDatabaseFixture.Db.Users.Find(x => x.Id == 2);
            Assert.False(user.Deleted);
            Assert.Equal(user.Name, "TestName1");
        }

        [Fact]
        public async Task FindAsync()
        {
            const int id = 4;
            const string name = "TestName3";
            {
                var user = await _sqlDatabaseFixture.Db.Users.FindAsync(x => x.Id == id);
                Assert.False(user.Deleted);
                Assert.Equal(user.Name, name);
            }
            {
                var user = await _sqlDatabaseFixture.Db.Users.FindAsync(x => x.Name == name);
                Assert.Equal(user.Id, id);

                Assert.Null(user.Cars);
            }
        }

        [Fact]
        public async Task FindAllAsync()
        {
            var users = (await _sqlDatabaseFixture.Db.Users.FindAllAsync(x => x.Name == "TestName0")).ToArray();
            Assert.Equal(users.Count(), 2);

            var user1 = users.FirstOrDefault(x => x.Id == 1);
            Assert.NotNull(user1);

            var user2 = users.FirstOrDefault(x => x.Id == 2);
            Assert.Null(user2);

            var user11 = users.FirstOrDefault(x => x.Id == 11);
            Assert.NotNull(user11);

        }


        [Fact]
        public async Task FindJoinAsync()
        {
            var user = await _sqlDatabaseFixture.Db.Users.FindAsync<Car>(x => x.Id == 1, q => q.Cars);
            Assert.Equal(user.Cars.Count, 1);
            Assert.Equal(user.Cars.First().Name, "TestCar0");

            var car = await _sqlDatabaseFixture.Db.Cars.FindAsync<User>(x => x.Id == 1, q => q.User);
            Assert.Equal(car.User.Name, "TestName0");

        }

        [Fact]
        public async Task InsertAndUpdateAsync()
        {
            var user = new User()
            {
                Name = "Sergey",
            };

            var insert = await _sqlDatabaseFixture.Db.Users.InsertAsync(user);
            Assert.True(insert);

            var userFromDb = await _sqlDatabaseFixture.Db.Users.FindAsync(q => q.Id == user.Id);
            Assert.Equal(userFromDb.Name, user.Name);
            user.Name = "Sergey1";

            var update = await _sqlDatabaseFixture.Db.Users.UpdateAsync(user);
            Assert.True(update);
            userFromDb = await _sqlDatabaseFixture.Db.Users.FindAsync(q => q.Id == user.Id);
            Assert.Equal(userFromDb.Name, "Sergey1");
        }

        [Fact]
        public async Task LogicalDeletedAsync()
        {
            const int id = 10;

            var user = _sqlDatabaseFixture.Db.Users.Find(x => x.Id == id);
            Assert.False(user.Deleted);

            var deleted = await _sqlDatabaseFixture.Db.Users.DeleteAsync(user);
            Assert.True(deleted);

            var deletedUser = _sqlDatabaseFixture.Db.Users.Find(x => x.Id == id);
            Assert.True(deletedUser.Deleted);
        }
    }
}
