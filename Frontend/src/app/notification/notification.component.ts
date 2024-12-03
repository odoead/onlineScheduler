import { Component, EventEmitter } from '@angular/core';
import { NotificationService } from '../global/services/notification.service';
import {  Notification_ } from '../global/models/notification';
import {  MatSidenavModule } from '@angular/material/sidenav';
import {  MatToolbarModule } from '@angular/material/toolbar';
import {  MatListModule } from '@angular/material/list';
import {  MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [MatSidenavModule,MatToolbarModule,MatListModule,MatIconModule,CommonModule],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent {
  isOpen = false;
  toggle = new EventEmitter<void>();
  notifications: Notification_[]  = [];
  private page: number = 0;
  private pageSize: number = 20;
  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadActivities();
  }
  /* 
  onToggleChange(event: any) {
    this.toggle.emit();
  }*/
  loadActivities(): void {
      this.notificationService.getNotifications(this.page,this.pageSize,true).subscribe((notifications)=>
        {this.notifications= notifications});
    }

  loadMore(): void {
    this.page++;
    this.notificationService.getNotifications(this.page,this.pageSize,false).subscribe((notifications)=>
      {notifications.length>0?   this.notifications.push(...notifications):  this.notifications=this.notifications});
    
  } 
  objectKeys(obj: any): string[] {
    return Object.keys(obj);
  }
}
