import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HeadersService } from './headerService';
import {  Notification_ } from '../models/notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  baseUrl= environment.bookingUrl;
  constructor(private http: HttpClient) { }

  public getNotifications(pageNumber: number, pageSize: number,isPageLoad:boolean): Observable<Notification_[]> {
    let params = new HttpParams()
    .set('pageNumber', pageNumber.toString())
    .set('pageSize', pageSize.toString());
    const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
    return this.http.get<Notification_[]>(`${this.baseUrl}/api/notification/`,{headers:headers,params:params});
  }
}
