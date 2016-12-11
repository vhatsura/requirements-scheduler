import { Component, OnInit } from '@angular/core';
import { User, Experiment } from "../../models/index";
import { ExperimentService, AlertService } from "../../services/index";
import { isBrowser } from 'angular2-universal';

@Component({
    selector: 'home',
    template: require('./home.component.html')
})

export class HomeComponent implements OnInit {
    model: any = {};
    error = '';
    success = '';
    loading = false;
    currentUser: User;

    constructor(
        private experimentService: ExperimentService,
        private alertService: AlertService
    ) { }

    create() {
        this.loading = true;
        this.experimentService.create(this.model)
            .subscribe(result => {
                if (result.status === 200) {
                    this.model = {};
                    this.success = "Experiment was submitted successfully";
                    this.error = '';
                } else {
                    this.error = result.response;
                }
                this.loading = false;
            });
    }

    ngOnInit(): void {
        if (isBrowser) {
            this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
        }
    }
}
