import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { UserMin } from '../../global/models/userMin';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TimeSpanService } from '../../global/services/timeSpanService';

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

    this.selectedWorkerIds = this.data.product.Workers.map(
      (worker) => worker.Id
    );

    this.form = this.fb.group({
      name: [
        this.data.product.Name,
        [Validators.required, Validators.minLength(3)],
      ],
      description: [this.data.product.Description, Validators.required],
      duration: [this.data.product.Duration, Validators.required],
      workerIds: [this.selectedWorkerIds, Validators.required],
    });
  }

  save() {
    if (this.form.valid) {
      const formValue = this.form.value;

      const updatedProduct: UpdateProduct = {
        Name: formValue.name,
        Description: formValue.description,
        Duration:this.timespanService.formatTimePicker( formValue.duration),
        WorkerIds: formValue.workerIds,
      };

      this.dialogRef.close(updatedProduct);
    }
  }

  cancel() {
    this.dialogRef.close(null);
  }
}
