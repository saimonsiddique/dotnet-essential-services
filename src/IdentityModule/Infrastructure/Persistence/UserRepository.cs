﻿using BaseModule.Application.DTOs.Responses;
using BaseModule.Application.Providers.Interfaces;
using BaseModule.Infrastructure.Extensions;
using IdentityModule.Application.Commands;
using IdentityModule.Application.DTOs;
using IdentityModule.Application.Providers.Interfaces;
using IdentityModule.Application.Queries;
using IdentityModule.Domain.Entities;
using IdentityModule.Domain.Repositories.Interfaces;
using MongoDB.Driver;

namespace IdentityModule.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        #region Fields

        private readonly IMongoDbContextProvider _mongoDbContextProvider;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthenticationContextProvider _authenticationContextProvider;

        #endregion

        #region Ctor

        public UserRepository(IMongoDbContextProvider mongoDbService, IRoleRepository roleRepository, IAuthenticationContextProvider authenticationContext)
        {
            _mongoDbContextProvider = mongoDbService;
            _roleRepository = roleRepository;
            _authenticationContextProvider = authenticationContext;
        }

        #endregion

        #region Methods

        public async Task<ServiceResponse> CreateUser(User user, string[] roles)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();

            var userRoleMaps = new List<UserRoleMap>();

            // if roles were sent map user to role
            if (roles != null && roles.Any())
            {
                var foundRoles = await _roleRepository.GetRolesByNames(roles);

                foreach (var role in foundRoles)
                {
                    var roleMap = new UserRoleMap()
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                    };

                    userRoleMaps.Add(roleMap);
                }
            }

            await _mongoDbContextProvider.InsertDocument(user);

            if (userRoleMaps.Any())
                await _mongoDbContextProvider.InsertDocuments(userRoleMaps);

            return Response.BuildServiceResponse().BuildSuccessResponse(user, authCtx?.RequestUri);
        }

        public async Task<ServiceResponse> UpdateUser(User user)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();

            var update = Builders<User>.Update
                .Set(x => x.FirstName, user.FirstName)
                .Set(x => x.LastName, user.LastName)
                .Set(x => x.ProfileImageUrl, user.ProfileImageUrl)
                .Set(x => x.Address, user.Address)
                .Set(x => x.TimeStamp.ModifiedOn, DateTime.UtcNow)
                .Set(x => x.TimeStamp.ModifiedBy, authCtx.User?.Id);

            await _mongoDbContextProvider.UpdateById(update: update, id: user.Id);
            var updatedUser = await _mongoDbContextProvider.FindById<User>(user.Id);

            return Response.BuildServiceResponse().BuildSuccessResponse(updatedUser, authCtx?.RequestUri);
        }

        public async Task<ServiceResponse> UpdateUserPassword(string userId, string oldPassword, string newPassword)
        {
            return await UpdateUserPasswordById(userId: userId, password: newPassword);
        }

        public async Task<ServiceResponse> UpdateUserPasswordById(string userId, string password)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();

            var update = Builders<User>.Update.Set(x => x.Password, password.Encrypt());

            var updatedUser = await _mongoDbContextProvider.UpdateById(update: update, id: userId);

            return Response.BuildServiceResponse().BuildSuccessResponse(updatedUser, authCtx?.RequestUri);
        }

        public async Task<ServiceResponse> UpdateUserRoles(string userId, string[] roleNames)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();

            var exisitingUserRoleMaps = await _mongoDbContextProvider.GetDocuments(Builders<UserRoleMap>.Filter.Eq(x => x.UserId, userId));

            var roles = await _roleRepository.GetRolesByNames(roleNames);

            var newUserRoleMaps = new List<UserRoleMap>();

            foreach (var role in roles)
            {
                var roleMap = new UserRoleMap()
                {
                    UserId = userId,
                    RoleId = role.Id,
                };

                newUserRoleMaps.Add(roleMap);
            }

            if (exisitingUserRoleMaps.Any())
                await _mongoDbContextProvider.DeleteDocuments(Builders<UserRoleMap>.Filter.In(x => x.Id, exisitingUserRoleMaps.Select(x => x.Id).ToArray()));

            if (newUserRoleMaps.Any())
                await _mongoDbContextProvider.InsertDocuments(newUserRoleMaps);

            return Response.BuildServiceResponse().BuildSuccessResponse(newUserRoleMaps, authCtx?.RequestUri);
        }

        public async Task<bool> BeAnExistingUserEmail(string userEmail)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, userEmail);

            return await _mongoDbContextProvider.Exists(filter);
        }

        public async Task<bool> BeValidUserPassword(string userId, string password)
        {
            var filter = Builders<User>.Filter.And(Builders<User>.Filter.Eq(x => x.Id, userId), Builders<User>.Filter.Eq(x => x.Password, password.Encrypt()));

            return await _mongoDbContextProvider.Exists(filter);
        }

        public async Task<bool> BeAnExistingPhoneNumber(string phoneNumber)
        {
            var filter = Builders<User>.Filter.Eq(x => x.PhoneNumber, phoneNumber);

            return await _mongoDbContextProvider.Exists(filter);
        }

        public async Task<bool> BeValidUser(string userEmail, string password)
        {
            var encryptedPassword = password.Encrypt();

            var filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq(x => x.Email, userEmail),
                    Builders<User>.Filter.Eq(x => x.Password, encryptedPassword));

            return await _mongoDbContextProvider.Exists(filter);
        }

        public async Task<User> GetUser(string userEmail, string password)
        {
            var encryptedPassword = password.Encrypt();

            var filter = Builders<User>.Filter.And(
                   Builders<User>.Filter.Eq(x => x.Email, userEmail),
                   Builders<User>.Filter.Eq(x => x.Password, encryptedPassword));

            return await _mongoDbContextProvider.FindOne(filter);
        }

        public async Task<User> GetUserByEmail(string userEmail)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, userEmail);

            return await _mongoDbContextProvider.FindOne(filter);
        }

        public async Task<User> GetUserById(string userId)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            return await _mongoDbContextProvider.FindOne(filter);
        }

        public async Task<QueryRecordResponse<UserResponse>> GetUser(string userId)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();

            var filter = Builders<User>.Filter.Eq(x => x.Id, userId);

            var user = await _mongoDbContextProvider.FindOne(filter);

            return Response.BuildQueryRecordResponse<UserResponse>().BuildSuccessResponse(UserResponse.Initialize(user), authCtx?.RequestUri);
        }

        public async Task<QueryRecordsResponse<UserResponse>> GetUsers(string searchTerm, int pageIndex, int pageSize)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();

            var filter = Builders<User>.Filter.Empty;

            if (!searchTerm.IsNullOrBlank())
            {
                filter &= Builders<User>.Filter.Or(
                    Builders<User>.Filter.Where(x => x.FirstName.ToLower().Contains(searchTerm.ToLower())),
                    Builders<User>.Filter.Where(x => x.LastName.ToLower().Contains(searchTerm.ToLower())),
                    Builders<User>.Filter.Where(x => x.DisplayName.ToLower().Contains(searchTerm.ToLower())),
                    Builders<User>.Filter.Where(x => x.Email.ToLower().Contains(searchTerm.ToLower())));
            }

            var count = await _mongoDbContextProvider.CountDocuments(filter: filter);

            var users = await _mongoDbContextProvider.GetDocuments(
                filter: filter,
                skip: pageIndex * pageSize,
                limit: pageSize);

            return new QueryRecordsResponse<UserResponse>().BuildSuccessResponse(
               count: count,
               records: users is not null ? users.Select(x => UserResponse.Initialize(x)).ToArray() : Array.Empty<UserResponse>(), authCtx?.RequestUri);
        }

        public async Task<bool> BeAnExistingUser(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            return await _mongoDbContextProvider.Exists(filter);
        }

        public async Task<bool> ActivateUser(string id)
        {
            var updateStatus = Builders<User>.Update.Set(x => x.UserStatus, UserStatus.Active);
            var updatedUser = await _mongoDbContextProvider.UpdateById(update: updateStatus, id);

            return updatedUser is not null;
        }

        public async Task<ServiceResponse> SubmitUser(User user)
        {
            var authCtx = _authenticationContextProvider.GetAuthenticationContext();            

            await _mongoDbContextProvider.InsertDocument(user);

            return Response.BuildServiceResponse().BuildSuccessResponse(user, authCtx?.RequestUri);
        }

        #endregion
    }
}