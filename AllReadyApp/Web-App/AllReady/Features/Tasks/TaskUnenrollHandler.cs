﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllReady.Features.Notifications;
using AllReady.Models;
using MediatR;
using Microsoft.Data.Entity;

namespace AllReady.Features.Tasks
{
    public class TaskUnenrollHandler : IAsyncRequestHandler<TaskUnenrollCommand, TaskUnenrollResult>
    {
        private readonly IMediator _bus;
        private readonly AllReadyContext _context;

        public TaskUnenrollHandler(IMediator bus, AllReadyContext context)
        {
            _bus = bus;
            _context = context;
        }

        public async Task<TaskUnenrollResult> Handle(TaskUnenrollCommand message)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedVolunteers)
                .Include(t => t.Event).ThenInclude(a => a.UsersSignedUp)
                .Include(t => t.RequiredSkills).ThenInclude(s => s.Skill)
                .SingleOrDefaultAsync(c => c.Id == message.TaskId);

            var campaignEvent = task.Event;

            var taskSignUp = task.AssignedVolunteers.SingleOrDefault(a => a.User.Id == message.UserId);
            if (taskSignUp != null)
                task.AssignedVolunteers.Remove(taskSignUp);

            if (campaignEvent.EventType != EventTypes.EventManaged && !campaignEvent.IsUserInAnyTask(message.UserId))
            {
                var eventSignup = campaignEvent.UsersSignedUp.FirstOrDefault(u => u.User.Id == message.UserId);
                if (eventSignup != null)
                    campaignEvent.UsersSignedUp.Remove(eventSignup);
            }
            
            await _context.SaveChangesAsync();

            await _bus.PublishAsync(new UserUnenrolls { EventId = campaignEvent.Id, UserId = message.UserId, TaskIds = new List<int> {task.Id} });

            return new TaskUnenrollResult { Status = "success", Task = task};
        }
    }
}