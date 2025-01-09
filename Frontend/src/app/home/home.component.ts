import { Component, OnInit } from '@angular/core';
import { CompanyMin } from '../global/models/companyMin';
import { CompanyService } from '../global/services/company.service';
import { Router } from 'express';
import { MatListModule } from '@angular/material/list';
import { CompanyListComponent } from '../company/company-list/company-list.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [MatListModule,CompanyListComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent   {
 
  constructor( ) {}

   


}
