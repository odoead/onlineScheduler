import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserMin } from '../../global/models/userMin';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TimeSpanService } from '../../global/services/timeSpanService';
import { ProductService } from '../../global/services/product.service';
import { Product } from '../../global/models/product';
import { UpdateProduct } from '../../global/models/updateProduct';

@Component({
  selector: 'app-edit-booking',
  standalone: true,
  imports: [],
  templateUrl: './edit-booking.component.html',
  styleUrl: './edit-booking.component.css'
})
export class EditBookingComponent {
  form!: FormGroup;
  workers: UserMin[] = [];
  selectedWorkerIds: string[] = [];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<EditBookingComponent>,
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
