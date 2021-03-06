﻿using Strive.Model;

namespace Strive.Data.Events
{
    public class TaskCompleteEvent : Event
    {
        public TaskCompleteEvent(TaskModel task, EntityModel doer, string description)
        {
            Task = task;
            Doer = doer;
            Description = description;
        }

        public TaskModel Task { get; set; }
        public EntityModel Doer { get; set; }
    }
}
