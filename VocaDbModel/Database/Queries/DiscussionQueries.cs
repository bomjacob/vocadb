﻿using VocaDb.Model.Database.Repositories;
using VocaDb.Model.DataContracts;
using VocaDb.Model.DataContracts.Discussions;
using VocaDb.Model.DataContracts.Users;
using VocaDb.Model.Domain.Comments;
using VocaDb.Model.Domain.Discussions;
using VocaDb.Model.Domain.Security;
using VocaDb.Model.Domain.Users;
using VocaDb.Model.Service;
using VocaDb.Model.Service.Queries;

namespace VocaDb.Model.Database.Queries {

	public class DiscussionQueries : QueriesBase<IDiscussionFolderRepository, DiscussionFolder> {

		private readonly IEntryLinkFactory entryLinkFactory;
		private readonly IUserIconFactory userIconFactory;

		private ICommentQueries Comments<T>(IDatabaseContext<T> ctx) {
			return new CommentQueries<DiscussionComment, DiscussionTopic>(ctx.OfType<DiscussionComment>(), PermissionContext, userIconFactory, entryLinkFactory);
		}

		public DiscussionQueries(IDiscussionFolderRepository repository, IUserPermissionContext permissionContext, IUserIconFactory userIconFactory,
			IEntryLinkFactory entryLinkFactory) 
			: base(repository, permissionContext) {
			
			this.userIconFactory = userIconFactory;
			this.entryLinkFactory = entryLinkFactory;

		}

		public CommentForApiContract CreateComment(int topicId, CommentForApiContract contract) {
			
			return repository.HandleTransaction(ctx => Comments(ctx).Create(topicId, contract));

		}

		public DiscussionFolderContract CreateFolder(DiscussionFolderContract contract) {
			
			// TODO
			PermissionContext.VerifyPermission(PermissionToken.Admin);

			return repository.HandleTransaction(ctx => {
				
				var folder = new DiscussionFolder {
					Name = contract.Name
				};

				ctx.Save(folder);

				ctx.AuditLogger.AuditLog("created " + folder);

				return new DiscussionFolderContract(folder, DiscussionFolderOptionalFields.None, userIconFactory);

			});

		}

		public DiscussionTopicContract CreateTopic(int folderId, DiscussionTopicContract contract) {
			
			PermissionContext.VerifyPermission(PermissionToken.CreateComments);

			if (contract.Author == null || contract.Author.Id != PermissionContext.LoggedUserId) {
				throw new NotAllowedException("Can only post as self");
			}

			return repository.HandleTransaction(ctx => {
				
				var folder = ctx.Load(folderId);
				var agent = ctx.OfType<User>().CreateAgentLoginData(PermissionContext, ctx.OfType<User>().Load(contract.Author.Id));

				var topic = new DiscussionTopic(folder, contract.Name, contract.Content, agent);
				folder.AllTopics.Add(topic);

				ctx.Save(topic);

				ctx.AuditLogger.AuditLog("created " + topic, agent);

				return new DiscussionTopicContract(topic, userIconFactory, DiscussionTopicOptionalFields.None);

			});

		}

		public void DeleteComment(int commentId) {
			
			repository.HandleTransaction(ctx => Comments(ctx).Delete(commentId));

		}

		/// <summary>
		/// Soft-deletes a discussion topic.
		/// The topic is marked as deleted, not actually removed from the DB.
		/// User can delete their own topics, moderators can delete all topics.
		/// </summary>
		/// <param name="topicId">Id of the topic to be deleted.</param>
		public void DeleteTopic(int topicId) {
			
			repository.HandleTransaction(ctx => {

				var topic = ctx.OfType<DiscussionTopic>().Load(topicId);
				var user = ctx.OfType<User>().GetLoggedUser(PermissionContext);

				ctx.AuditLogger.AuditLog("deleting " + topic, user);

				if (!user.Equals(topic.Author))
					PermissionContext.VerifyPermission(PermissionToken.DeleteComments);

				topic.Deleted = true;
				ctx.Update(topic);

			});

		}

		private void MoveTopic(IDatabaseContext ctx, DiscussionTopic topic, int targetFolderId) {

			PermissionContext.VerifyPermission(PermissionToken.DeleteComments);

			var folder = ctx.Load<DiscussionFolder>(targetFolderId);

			ctx.AuditLogger.AuditLog(string.Format("Moving {0} to {1}", topic, folder));

			topic.MoveToFolder(folder);

		}

		public void UpdateComment(int commentId, IComment contract) {
			
			repository.HandleTransaction(ctx => Comments(ctx).Update(commentId, contract));

		}

		public void UpdateTopic(int topicId, DiscussionTopicContract contract) {
			
			PermissionContext.VerifyPermission(PermissionToken.CreateComments);

			repository.HandleTransaction(ctx => {
				
				var topic = ctx.OfType<DiscussionTopic>().Load(topicId);

				PermissionContext.VerifyAccess(topic, EntryPermissionManager.CanEdit);

				if (topic.Folder.Id != contract.FolderId && PermissionContext.HasPermission(PermissionToken.DeleteComments)) {
					MoveTopic(ctx, topic, contract.FolderId);
				}

				topic.Name = contract.Name;
				topic.Content = contract.Content;

				ctx.Update(topic);
				ctx.AuditLogger.AuditLog("updated " + topic);

			});

		}

	}

}