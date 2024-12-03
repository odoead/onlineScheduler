import { Component, Inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { CreateProduct } from '../../global/models/createProduct';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductService } from '../../global/services/product.service';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UserMin } from '../../global/models/userMin';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatOption, MatSelect } from '@angular/material/select';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { FormValidationDirective } from '../../global/directives/form-validation.directive';
import { TimeSpanService } from '../../global/services/timeSpanService';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [MatCardModule,MatFormField,MatLabel,MatSelect,MatOption,ReactiveFormsModule,FormValidationDirective,NgxMaterialTimepickerModule],
  templateUrl: './add-product.component.html',
  styleUrl: './add-product.component.css'
})
export class AddProductComponent {
  form!: FormGroup;
  workers: UserMin[] = []; 
  companyId!: number; 

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private timespanService:TimeSpanService,
    public dialogRef: MatDialogRef<AddProductComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {companyId:number, workers: UserMin[] }
  ) {}

  ngOnInit(): void {
    this.workers = this.data.workers;
    this.companyId = this.data.companyId;
    
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      duration: ['', Validators.required], 
      workerIds: [this.workers.map((w)=>w.Id), Validators.required], 
    });
  }

  save(): void {
    if (this.form.valid) {
      const formValue = this.form.value;

      const newProduct: CreateProduct = {
        Name: formValue.name,
        Description: formValue.description,
        Duration: this.timespanService.formatTimePicker(formValue.duration),
        CompanyId: this.companyId,
        WorkerIds: formValue.workerIds,
      };

      this.productService.addProduct(newProduct).subscribe({
        next: (productId:number) => {
          alert(`Product created successfully with ID: ${productId}`);
          this.dialogRef.close(true); 
        },
        error: () => {
          alert('Failed to create product');
        },
      });
    }
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
