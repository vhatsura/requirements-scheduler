import { Component, OnInit } from '@angular/core';
import { GtExpandedRow } from 'angular-generic-table';

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { CustomValidators } from 'ng2-validation';

@Component({
    selector: 'user-detail',
    styles: [`
    .ng-valid[required], .ng-valid.required  {
        border-left: 5px solid #42A948; /* green */
    }
    .ng-invalid:not(form)  {
        border-left: 5px solid #a94442; /* red */
    }
    .vcenter {
        display: inline-block;
        vertical-align: middle;
        float: none;
    }
    .padding-0{
        padding-right:0;
        padding-left:0;
    }
    `],
    templateUrl: './user-detail.component.html'
})
export class UserDetailComponent extends GtExpandedRow<any> implements OnInit {
    
    f: FormGroup;

    constructor(private formBuilder: FormBuilder) { super(); }

    ngOnInit() {
       this.f = this.formBuilder.group({
           issuerName: ['', Validators.required],
           expTime: ['', Validators.required],
           signKey: ['FB8A0C93-FE6C-4EA4-9869-81F17A1CF917', Validators.required],
           cloudUrl: ['', Validators.required]
            // testsAmount: ['', Validators.compose([Validators.required, CustomValidators.min(1)])],
            // requirementsAmount: ['', Validators.compose([Validators.required, CustomValidators.min(1)])],
            // n1: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            // n2: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            // n12: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            // n21: ['', Validators.compose([Validators.required, CustomValidators.range([0, 100])])],
            // minBoundaryRange: ['', Validators.compose([Validators.required, CustomValidators.min(0)])],
            // maxBoundaryRange: ['', Validators.compose([Validators.required, CustomValidators.min(0)])],
            // minPercentageFromA: ['', Validators.compose([Validators.required, CustomValidators.range([5, 50])])],
            // maxPercentageFromA: ['', Validators.compose([Validators.required, CustomValidators.range([5, 50])])],
            // borderGenerationType: ['', Validators.required],
            // pGenerationType: ['', Validators.required]
        }); 
    }
}
