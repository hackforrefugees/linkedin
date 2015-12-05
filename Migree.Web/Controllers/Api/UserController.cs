﻿using Migree.Core.Interfaces;
using Migree.Web.Models.Requests;
using Migree.Web.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Migree.Web.Controllers.Api
{
    [RoutePrefix("api/user")]
    public class UserController : MigreeApiController
    {
        private const int NUMBER_OF_MATCHES_TO_TAKE = 50;

        private IUserServant UserServant { get; }
        private ICompetenceServant CompetenceServant { get; }

        public UserController(IUserServant userServant, ICompetenceServant comptenceServant)
        {
            UserServant = userServant;
            CompetenceServant = comptenceServant;
        }

        [HttpGet]
        [Route("{userId:guid}/competences")]
        public HttpResponseMessage GetUserCompetences(Guid userId)
        {
            var competences = UserServant.GetUserCompetences(userId);
            var response = competences.Select(x => new IdAndNameResponse { Id = x.Id, Name = x.Name }).ToList();
            return CreateApiResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [Route("login")]
        public HttpResponseMessage Login(LoginRequest request)
        {
            var user = UserServant.FindUser(request.Email, request.Password);

            if (user != null)
            {
                return CreateApiResponse(HttpStatusCode.OK);
            }

            return CreateApiResponse(HttpStatusCode.Unauthorized);
        }
        [HttpPost]
        [Route("register")]
        public HttpResponseMessage Register(RegisterRequest request)
        {
            var user = UserServant.Register(request.Email, request.Password, request.FirstName, request.LastName, request.UserType);
            return CreateApiResponse(HttpStatusCode.OK, new RegisterResponse { UserId = user.Id });
        }

        [HttpPost, Route("{userId:guid}/upload", Name = "userimageupload")]
        public async Task<HttpResponseMessage> UploadProfileImage(Guid userId)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Unsupported media");
                }

                var content = await Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider());
                using (var imageStream = await content.Contents.First().ReadAsStreamAsync())
                {
                    await UserServant.UploadProfileImageAsync(userId, imageStream);
                }

                return CreateApiResponse(HttpStatusCode.Accepted);
            }
            catch
            {
                return CreateApiResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost, Route("{userId:guid}/matches")]
        public HttpResponseMessage FindMatches(Guid userId, FindMatchesRequest request)
        {
            var matchedUsers = CompetenceServant.GetMatches(userId, request.CompetenceIds, NUMBER_OF_MATCHES_TO_TAKE);

            var users = matchedUsers.Select(p => new UserMatchResponse
            {
                UserId = p.Id,
                FullName = $"{p.FirstName} {p.LastName}",
                Description = p.Description,
                UserLocation = p.UserLocation.ToDescription()
            }).ToList();

            return CreateApiResponse(HttpStatusCode.NoContent, users);
        }
        [HttpPut, Route("{userId:guid}")]
        public HttpResponseMessage Update(Guid userId, UpdateUserRequest request)
        {
            UserServant.UpdateUser(userId, request.UserLocation, request.Description);
            UserServant.AddCompetencesToUser(userId, request.CompetenceIds);
            return CreateApiResponse(HttpStatusCode.NoContent);
        }
    }
}
