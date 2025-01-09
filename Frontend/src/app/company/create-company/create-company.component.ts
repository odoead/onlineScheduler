import { Component, OnInit } from '@angular/core';
import { CompanyService } from '../../global/services/company.service';
import {
  FormBuilder,
  FormGroup,
  FormArray,
  FormControl,
  Validators,
} from '@angular/forms';
import { CreateCompany } from '../../global/models/createCompany';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { provideNativeDateAdapter } from '@angular/material/core';
import { NgxMaterialTimepickerTheme } from 'ngx-material-timepicker';
import { FormValidationDirective } from '../../global/directives/form-validation.directive';
import { TimeSpanService } from '../../global/services/timeSpanService';
import { companyType } from '../../global/models/companyType';
import { DayOfTheWeek } from '../../global/models/dayOfTheWeek';

@Component({
  selector: 'app-create-company',
  standalone: true,
  imports: [
    MatIconModule,
    MatCardModule,
    MatSelectModule,
    MatMenuModule,
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatInputModule,
    MatButtonModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatNativeDateModule,
    NgxMaterialTimepickerModule,FormValidationDirective
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './create-company.component.html',
  styleUrl: './create-company.component.css',
})
export class CreateCompanyComponent implements OnInit {
  form!: FormGroup;

  readonly COMPANY_TYPES = Object.entries(companyType).map(([key,value])=>({label: key, value}))

  readonly DAYS_OF_WEEK = Object.entries(DayOfTheWeek).map(([key,value])=>({label: key, value}))

  constructor(
    private companyService: CompanyService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private timespanService:TimeSpanService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.form = this.fb.group(
      {
        name: ['', [Validators.required, Validators.maxLength(100)]],
        description: ['', [Validators.maxLength(500)]],
        openingTimeLOC: [null, [Validators.required]],
        closingTimeLOC: [null, [Validators.required]],
        companyType: [null, Validators.required],
        workingDays: [[], Validators.required],
        latitude: [null,[Validators.required, Validators.min(-90), Validators.max(90)],],
        longitude: [null,[Validators.required, Validators.min(-180), Validators.max(180)],],
        ownerEmail: ['', [Validators.required, Validators.email]],
      }
    );
  }

  onSubmit(): void {
    const openingTime = this.form.get('openingTimeLOC')?.value;
    const closingTime = this.form.get('closingTimeLOC')?.value;
    if (openingTime && closingTime && openingTime < closingTime) {
      if (this.form.valid) {
        const companyData: CreateCompany = {
          ...this.form.value,
          openingTimeLOC: this.timespanService.formatTimePicker(this.form.value.openingTimeLOC),
          closingTimeLOC: this.timespanService.formatTimePicker(this.form.value.closingTimeLOC),
        };

        this.companyService.addCompany(companyData).subscribe({
          next: (companyId) => {
            this.snackBar.open('Company added successfully', 'Close', {
              duration: 3000,
            });
            this.router.navigate(['/companies', companyId]);
          },
          error: (error) => {
            this.snackBar.open('Failed to add company', 'Close', {
              duration: 3000,
            });
            console.error('Error adding company', error);
          },
        });
      }
    }
  }

  
}
