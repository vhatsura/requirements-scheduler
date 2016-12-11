import { Component, Injectable } from '@angular/core';
import { Response } from '@angular/http';
import { AuthHttp } from 'angular2-jwt';
import { HttpResponse } from '../models/index';
import { Observable } from 'rxjs/Observable';
import { Experiment } from '../models/index';

@Injectable()
export class ExperimentService {
    constructor(private authHttp: AuthHttp) { }

    create(experiment: Experiment) : Observable<HttpResponse> {
        return this.authHttp.post('/api/experiments', experiment).map((response: Response) => {
            let httpResponse = new HttpResponse();
            httpResponse.status = response.status;
            httpResponse.response = response.json();
            return httpResponse;
        });
    }
}