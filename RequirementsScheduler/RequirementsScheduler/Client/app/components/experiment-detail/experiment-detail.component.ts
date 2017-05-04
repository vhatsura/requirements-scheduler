import { Component, OnInit, Input } from '@angular/core';
import { GtExpandedRow } from 'angular-generic-table';

import { Test } from '../../models/index';

import { ExperimentService } from '../../services/index';

@Component({
    selector: 'experiment-detail',
    templateUrl: './experiment-detail.component.html'
})
export class ExperimentDetailComponent extends GtExpandedRow<any> implements OnInit {

    selectedTest: Test;
    tests: Test[];

    constructor(private _experimentService: ExperimentService) { super(); }

    ngOnInit() {
        console.log(this.row);
        this._experimentService.getExperimentResults(this.row.id)
            .subscribe(tests => {this.tests = tests; });
    }
}
