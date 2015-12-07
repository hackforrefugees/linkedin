﻿using Migree.Core.Definitions;
using Migree.Core.Exceptions;
using Migree.Core.Interfaces;
using Migree.Core.Interfaces.Models;
using Migree.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Migree.Core.Servants
{
    public class UserServant : IUserServant
    {
        private const int PROFILE_IMAGE_SIZE_PIXELS = 100;
        private IDataRepository DataRepository { get; }
        private IContentRepository ContentRepository { get; }
        private IPasswordServant PasswordServant { get; }
        private ICompetenceServant CompetenceServant { get; }
        private IMailRepository MailServant { get; }
        public UserServant(IDataRepository dataRepository, IPasswordServant passwordServant, ICompetenceServant competenceServant, IContentRepository contentRepository, IMailRepository mailServant)
        {
            DataRepository = dataRepository;
            PasswordServant = passwordServant;
            CompetenceServant = competenceServant;
            ContentRepository = contentRepository;
            MailServant = mailServant;
        }

        public IUser FindUser(string email, string password)
        {
            email = email.ToLower();
            var user = DataRepository.GetAll<User>().FirstOrDefault(p => p.Email.Equals(email));

            if (user == null || !PasswordServant.ValidatePassword(password, user.Password))
            {
                throw new ValidationException("Invalid credentials");
            }

            return user;
        }

        public IUser GetUser(Guid userId)
        {
            var user = DataRepository.GetFirstOrDefaultByRowKey<User>(User.GetRowKey(userId));
            return user;
        }

        public IUser Register(string email, string password, string firstName, string lastName, UserType userType)
        {
            email = email.ToLower();
            var user = new User(userType);
            user.Email = email;
            user.Password = PasswordServant.CreateHash(password);
            user.FirstName = firstName;
            user.LastName = lastName;
            user.UserType = userType;
            user.UserLocation = UserLocation.None;
            user.Description = string.Empty;
            DataRepository.AddOrUpdate(user);
            return user;
        }

        public void UpdateUser(Guid userId, UserLocation userLocation, string description)
        {
            var user = DataRepository.GetFirstOrDefaultByRowKey<User>(User.GetRowKey(userId));
            user.UserLocation = userLocation;
            user.Description = description;
            DataRepository.AddOrUpdate(user);
        }

        public void AddCompetencesToUser(Guid userId, ICollection<Guid> competenceIds)
        {
            var rowKey = userId.ToString();
            var oldCompetences = DataRepository.GetAllByRowKey<UserCompetence>(rowKey);

            foreach (var oldCompetence in oldCompetences)
            {
                DataRepository.Delete(oldCompetence);
            }

            foreach (var competenceId in competenceIds)
            {
                var userCompetence = new UserCompetence(userId, competenceId);
                DataRepository.AddOrUpdate(userCompetence);
            }
        }

        public ICollection<ICompetence> GetUserCompetences(Guid userId)
        {
            var competences = CompetenceServant.GetCompetences();
            var userCompetences = DataRepository.GetAllByRowKey<UserCompetence>(UserCompetence.GetRowKey(userId));
            return userCompetences.Select(p => new IdAndName { Id = p.CompetenceId, Name = competences.First(q => q.Id.Equals(p.CompetenceId)).Name }).ToList<ICompetence>();
        }

        public async Task UploadProfileImageAsync(Guid userId, Stream imageStream)
        {
            imageStream.Position = 0;

            using (var img = System.Drawing.Image.FromStream(imageStream))
            {
                using (var bitmap = new System.Drawing.Bitmap(img, PROFILE_IMAGE_SIZE_PIXELS, PROFILE_IMAGE_SIZE_PIXELS))
                {
                    using (var profileImageStream = new MemoryStream())
                    {
                        bitmap.Save(profileImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        await ContentRepository.PutImageAsync(userId, profileImageStream, ImageType.Profile);
                    }
                }
            }
        }

        public async Task SendMessageToUserAsync(Guid creatorUserId, Guid receiverUserId, string message)
        {
            AddMessage(creatorUserId, receiverUserId, message);

            var creatorUser = DataRepository.GetFirstOrDefaultByRowKey<User>(User.GetRowKey(creatorUserId));
            var receiverUser = DataRepository.GetFirstOrDefaultByRowKey<User>(User.GetRowKey(receiverUserId));
            var subject = $"You got a Migree-mail from {creatorUser.FullName}";
            message += "\n\n" + $"Reply to this e-mail or send a mail directly to {creatorUser.Email}, to get in touch with {creatorUser.FullName}";
            await MailServant.SendMailAsync(subject, message, receiverUser.Email, "no-reply@migree.se", $"{creatorUser.FullName} thru Migree", creatorUser.Email);
        }

        public string GetProfileImageUrl(Guid userId)
        {
            return ContentRepository.GetImageUrl(userId, ImageType.Profile);
        }

        private void AddMessage(Guid creatorUserId, Guid receiverUserId, string content)
        {
            var messageThread = DataRepository.GetFirstOrDefault<MessageThread>(
                MessageThread.GetPartitionKey(creatorUserId, receiverUserId),
                MessageThread.GetRowKey(creatorUserId, receiverUserId));

            var messageTimestamp = DateTime.UtcNow.Ticks;

            if (messageThread == null)
            {
                messageThread = new MessageThread(creatorUserId, receiverUserId);
                messageThread.LatestReadUser1 = messageThread.UserId1.Equals(creatorUserId) ? messageTimestamp : 0;
                messageThread.LatestReadUser2 = messageThread.UserId2.Equals(creatorUserId) ? messageTimestamp : 0;
            }

            var message = new Message(creatorUserId, receiverUserId)
            {
                Content = content,
                Created = messageTimestamp
            };

            messageThread.LatestMessageCreated = messageTimestamp;

            DataRepository.AddOrUpdate(message);
            DataRepository.AddOrUpdate(messageThread);            
        }
    }
}
