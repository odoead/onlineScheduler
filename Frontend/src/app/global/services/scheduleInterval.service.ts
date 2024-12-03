import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateInterval } from '../models/createInterval';
import { Observable } from 'rxjs';
import { TimeSpan } from '../models/timespan';
import { Company } from '../models/company';
import { ScheduleInterval } from '../models/scheduleInterval';
import { EmptyWindow } from '../models/emptyWindow';
import { HeadersService } from './headerService';
import { TimeSpanService } from './timeSpanService';

@Injectable({
  providedIn: 'root',
})
export class ScheduleService {
  baseUrl = environment.companyUrl;

  constructor(private http: HttpClient,private timespanService:TimeSpanService) {}

  public addInterval(createInterval: CreateInterval): Observable<number> {
    const headers =  HeadersService.getPageLoad();
    return this.http.post<number>(`${this.baseUrl}/api/scheduleinterval`,createInterval,{headers});
  }

  public deleteInterval(Id: number): Observable<boolean> {
    const headers =  HeadersService.getPageLoad();
    return this.http.delete<boolean>(`${this.baseUrl}/api/scheduleinterval/${Id}`,{headers});
  }

  updateInterval(id: number,startTimeLOC: TimeSpan,finishTimeLOC: TimeSpan): Observable<boolean> {
    const headers =  HeadersService.getPageLoad();
    const body = {startTimeLOC: startTimeLOC,FinishTimeLOC: finishTimeLOC,};
    return this.http.put<boolean>(`${this.baseUrl}/api/scheduleinterval/${id}`, body,{headers});
  }

  getWeeklyScheduleWithBookings( employeeId: string,currentDateLOC: string, isPageLoad: boolean): Observable<ScheduleInterval[]> {
    const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
    return this.http.get<ScheduleInterval[]>(`${this.baseUrl}/api/scheduleinterval/weekly/${employeeId}?currentDateLOC=${currentDateLOC}`
      ,{headers});
  }

  getWeeklyScheduleWithBookings_ForWorker( currentDateLOC: string, isPageLoad: boolean): Observable<ScheduleInterval[]> {
    const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
    return this.http.get<ScheduleInterval[]>(`${this.baseUrl}/api/scheduleinterval/weekly?currentDateLOC=${currentDateLOC}`,{headers});
  }

  getEmptyScheduleTimeByDate(employeeId: string,date: string , isPageLoad: boolean): Observable<EmptyWindow[]> {
    const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
    return this.http.get<EmptyWindow[]>(`${this.baseUrl}/api/scheduleinterval/available/${employeeId}?date=${date}`,{headers});
  }

  splitEmptyWindows(emptyWindows: EmptyWindow[], taskDuration: TimeSpan): EmptyWindow[] {
    const taskDurationMinutes = this.timespanService.getTimeSpanMinuteDuration(taskDuration);
    const newWindows: EmptyWindow[] = [];
  
    emptyWindows.forEach(w => {
      const windowStart = this.timespanService.getTimeSpanMinuteDuration(w.BeginTime);
      const windowDuration = this.timespanService.getTimeSpanMinuteDuration(w.Duration);
  
      if (windowDuration < taskDurationMinutes) {
        return;
      }
  
      let currentStart = windowStart;
      let remainingDuration = windowDuration;
  
      while (remainingDuration >= taskDurationMinutes) {
        newWindows.push({
          BeginTime: this.timespanService.minutesToTimeSpan(currentStart),
          EndTime: this.timespanService.minutesToTimeSpan(currentStart + taskDurationMinutes),
          Duration: taskDuration
        });
  
        currentStart += taskDurationMinutes;
        remainingDuration -= taskDurationMinutes;
      }
    });
  
    return newWindows;
  }


}
