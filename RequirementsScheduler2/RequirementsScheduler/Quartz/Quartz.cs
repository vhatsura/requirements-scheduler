﻿using System;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace RequirementsScheduler.Quartz
{
    internal class Quartz
    {
        private IScheduler _scheduler;

        public static IScheduler Scheduler => Instance._scheduler;

        private static readonly Lazy<Quartz> _instance = new Lazy<Quartz>(() => new Quartz());

        public static Quartz Instance => _instance.Value;

        private Quartz()
        {
            Init();
        }

        private async void Init()
        {
            _scheduler = await new StdSchedulerFactory().GetScheduler();
        }

        public IScheduler UseJobFactory(IJobFactory jobFactory)
        {
            Scheduler.JobFactory = jobFactory;
            return Scheduler;
        }

        public async void AddJob<T>(string name, string group, int interval)
            where T : IJob
        {
            var job = JobBuilder.Create<T>()
                .WithIdentity(name, group)
                .Build();

            var jobTrigger = TriggerBuilder.Create()
                .WithIdentity(name + "Trigger", group)
                .StartNow()
                .WithSimpleSchedule(t => t.WithIntervalInSeconds(interval).RepeatForever())
                .Build();

            await Scheduler.ScheduleJob(job, jobTrigger);
        }

        public static async void Start()
        {
            await Scheduler.Start();
        }
    }
}
