﻿import { Component, Injectable } from '@angular/core';
import { Response } from '@angular/http';
import { AuthHttp } from 'angular2-jwt';
import { HttpResponse } from '../models/index';
import { Observable } from 'rxjs/Observable';
import { Experiment, ExperimentStatus } from '../models/index';

@Injectable()
export class ExperimentService {
    constructor(private authHttp: AuthHttp) { }

    create(experiment: Experiment) : Observable<HttpResponse> {
        return this.authHttp.post('/api/experiments', experiment)
            .map((response: Response) => {
                let httpResponse = new HttpResponse();
                httpResponse.status = response.status;
                httpResponse.response = response.json();
                return httpResponse;
            });
    }

    getByStatus(status: ExperimentStatus): Observable<Array<Experiment>> {
        return Observable.create(observer => {
            this.authHttp.get('/api/experiments/GetByStatus/' + status)
                .map((response: Response) => response.json())
                .subscribe((result) => {
                    var experiments = new Array<Experiment>();
                    for (let r in result) {
                        if (result.hasOwnProperty(r)) {
                            experiments.push(new Experiment().deserialize(r));
                        }
                    }
                    observer.next(experiments);
                    observer.complete();
                });
        });
    }
}