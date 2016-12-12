import { Component, OnInit } from '@angular/core';

import { Experiment } from '../../models/index';
import { ExperimentService } from "../../services/index";
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { CustomValidators } from 'ng2-validation';

@Component({
    selector: 'experiment-form',
    styles: [`
    .ng-valid[required], .ng-valid.required  {
        border-left: 5px solid #42A948; /* green */
    }
    .ng-invalid:not(form)  {
        border-left: 5px solid #a94442; /* red */
    }
    `],
    template: require('./experiment-form.component.html')
})

export class ExperimentFormComponent implements OnInit {
    error = '';
    success = '';
    loading = false;
    f: FormGroup;

    constructor(
        private experimentService: ExperimentService,
        private formBuilder: FormBuilder
    ) { }

    onSubmit() {
        this.loading = true;
        this.experimentService.create(this.f.value)
            .subscribe(result => {
                if (result.status === 200) {
                    this.success = "Experiment was submitted successfully";
                    this.error = '';
                    this.f.reset();
                } else {
                    this.error = result.response;
                }
                this.loading = false;
            });
    }

    ngOnInit(): void {
        this.f = this.formBuilder.group({
            testsAmount: ['', Validators.compose([Validators.required, CustomValidators.min(1)])],
            requirementsAmount: ['', Validators.compose([Validators.required, CustomValidators.min(1)])],
            n1: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            n2: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            n12: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            n21: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            minBoundaryRange: ['', Validators.compose([Validators.required, CustomValidators.min(0)])],
            maxBoundaryRange: ['', Validators.compose([Validators.required, CustomValidators.min(0)])],
            borderGenerationType: ['', Validators.required],
            pGenerationType: ['', Validators.required]
        });
    }
}