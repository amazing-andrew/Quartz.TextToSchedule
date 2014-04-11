Quartz.TextToSchedule
-----------------------------------------------------------------

1. INTRODUCTION
----------------

Hello and welcome to a small pet project I have.

This is a library that is used to translate plain english into a schedule that can be used by Quartz.Net.

So you can do *really* cool things like create a scheduler using `every 30 minutes` or something more complex like `1st, 10th, and 20th day of month at 8am and 8pm`. Don't like to use plain english? That's fine too, you can simply use the same parser to process CRON strings! `0 15 10 * * ?`

Quartz.Net Library

    http://quartznet.sourceforge.net 
    http://github.com/quartznet/quartznet 


2. SCHEDULER FORMAT SYNTAX
---------------------

You can learn more about the scheduler format syntax by going to the following link

*[Scheduler Format Documentation](http://htmlpreview.github.io/?https://github.com/amazing-andrew/Quartz.TextToSchedule/blob/master/documentation/SchedulerFormat.htm)*


3. USAGE
----------------------

    IScheduler shed;
    IJobDetail jobDetail;
    
    TextToScheduleFactory factory = new TextToScheduleFactory();
    ITextToSchedule parser = factory.CreateStandardParser();
    
    TextToScheduleResults results = parser.Parse("every 30 minutes");
    
    //schedules the job to run every 30 minutes
    results.ScheduleWithJob(sched, jobDetail);


4. LICENSE
----------------

Licensed under the Apache License, Version 2.0 (the "License"); you may not 
use this file except in compliance with the License. You may obtain a copy 
of the License at 
 
    http://www.apache.org/licenses/LICENSE-2.0 
