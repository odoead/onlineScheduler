import { Component, Inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CompanyService } from '../../global/services/company.service';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { MatList, MatListItem } from '@angular/material/list';
import { MatIcon } from '@angular/material/icon';
import { MatError, MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { FormValidationDirective } from '../../global/directives/form-validation.directive';

@Component({
  selector: 'app-add-employee',
  standalone: true,
  imports: [MatCard,MatCardContent,MatCardHeader,MatCardTitle,MatCardSubtitle,MatList,MatListItem,MatIcon,MatCardActions,
    MatFormField,MatDialogActions,MatDialogContent,MatLabel,ReactiveFormsModule,MatFormFieldModule,FormValidationDirective
  ],
  templateUrl: './add-employee.component.html',
  styleUrl: './add-employee.component.css'
})
export class AddEmployeeComponent {
  employeeForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<AddEmployeeComponent>,
    private companyService: CompanyService,
    @Inject(MAT_DIALOG_DATA) public data: { companyId: number }) {
    this.employeeForm = this.fb.group({
      email:['', [Validators.required,Validators.email]]
    });
  }

  onSubmit() {
    if (this.employeeForm.valid) {
      const email:string[] =  ([this.employeeForm.get('email')?.value]);
      
        this.companyService.addEmployeesToCompany(this.data.companyId, email)
          .subscribe({
            next: () => {
              alert('Employees added successfully');
              this.dialogRef.close(true);
            },
            error: () => {
              alert('Failed to add employees');
            }
          });
    }
}
}
