import { Component } from '@angular/core';
import { ScheduleService } from '../global/services/scheduleInterval.service';
import { ScheduleInterval } from '../global/models/scheduleInterval';
import { TimeSpanService } from '../global/services/timeSpanService';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { TimeSpan } from '../global/models/timespan';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [MatCardModule,MatTableModule,RouterModule,CommonModule],
  templateUrl: './schedule.component.html',
  styleUrl: './schedule.component.css'
})
export class ScheduleComponent {
  weeklySchedule: ScheduleInterval[] = [];
  displayedColumns: string[] = ['day', 'intervals'];

  readonly DAYS_OF_WEEK = Object.entries(DayOfTheWeek).map(([key,value])=>({label: key, value}))

  constructor(private scheduleService: ScheduleService,private timespanService:TimeSpanService) {}

  ngOnInit(): void {
    const currentDateLOC = new Date().toISOString();
    this.scheduleService
      .getWeeklyScheduleWithBookings_ForWorker(currentDateLOC, true)
      .subscribe({
        next:  (schedule) => {
          this.weeklySchedule = schedule;
        },
        error:(error) => console.error('Error fetching schedule', error)
  });
  }

  getWeekdayName(weekDay: number): string {
    const day = this.DAYS_OF_WEEK.find(d => d.value === weekDay);
    return day ? day.label : '';
    }
  
  formatTime(time:TimeSpan)
  {
    return this.timespanService.formatTimeSpan(time);
  }
}
