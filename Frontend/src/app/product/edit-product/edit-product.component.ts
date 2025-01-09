import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Product } from '../../global/models/product';
import { UpdateProduct } from '../../global/models/updateProduct';
import {
  MatCard,
  MatCardContent,
  MatCardHeader,
  MatCardTitle,
} from '@angular/material/card';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { FormValidationDirective } from '../../global/directives/form-validation.directive';
import { UserMin } from '../../global/models/userMin';
import { TimeSpanService } from '../../global/services/timeSpanService';
import { ProductService } from '../../global/services/product.service';
import { DialogModule } from '@angular/cdk/dialog';

@Component({
  selector: 'app-edit-roduct',
  standalone: true,
  imports: [
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardContent,
    MatLabel,
    ReactiveFormsModule,
    MatFormField,
    MatInputModule,
    MatSelectModule,
    NgxMaterialTimepickerModule,
    FormValidationDirective,MatDialogModule
  ],
  templateUrl: './edit-product.component.html',
  styleUrl: './edit-product.component.css',
})
export class EditProductComponent implements OnInit {
  form!: FormGroup;
  workers: UserMin[] = [];
  selectedWorkerIds: string[] = [];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<EditProductComponent>,
    private productService:ProductService,
    private timespanService:TimeSpanService,
    @Inject(MAT_DIALOG_DATA)
    public data: { product: Product; workers: UserMin[] }
  ) {}

  ngOnInit(): void {
    this.workers = this.data.workers;

    this.selectedWorkerIds = this.data.product.workers.map(
      (worker) => worker.id
    );

    this.form = this.fb.group({
      name: [
        this.data.product.name,
        [Validators.required, Validators.minLength(3)],
      ],
      description: [this.data.product.description, Validators.required],
      duration: [this.data.product.duration, Validators.required],
      workerIds: [this.selectedWorkerIds, Validators.required],
    });
  }

  save() {
    if (this.form.valid) {
      const formValue = this.form.value;

      const updatedProduct: UpdateProduct = {
        name: formValue.name,
        description: formValue.description,
        duration:this.timespanService.formatTimePicker( formValue.duration),
        workerIds: formValue.workerIds,
      };

      this.dialogRef.close(updatedProduct);
    }
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
