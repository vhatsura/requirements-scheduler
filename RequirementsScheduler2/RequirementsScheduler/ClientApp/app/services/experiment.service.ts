import { Component, Injectable } from '@angular/core';
import { Response } from '@angular/http';
import { AuthHttp } from 'angular2-jwt';
import { HttpResponse } from '../models/index';
import { Observable } from 'rxjs/Observable';
import { Experiment, ExperimentStatus, Test } from '../models/index';

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

    createTest(experiment: any) : Observable<HttpResponse> {
        return this.authHttp.post('/api/experiments/test', experiment)
            .map((response: Response) => {
                let httpResponse = new HttpResponse();
                httpResponse.status = response.status;
                httpResponse.response = response.json();
                return httpResponse;
            });
    }

    getExperimentResults(id: string) : Observable<Array<Test>> {
        return Observable.create(observer => {
            this.authHttp.get(`/api/experiments/${id}/result`)
            .map((response: Response) => response.json())
            .subscribe(result => {
                var tests = new Array<Test>();
                
                console.log('Result of experiment');
                console.log(result);

                for(let r of result) {
                    tests.push(new Test().deserialize(r));
                }

                observer.next(tests);
                observer.complete();
            });
        });
    }

    getExperimentResult(id: string, testNumber: number) : Observable<Array<Test>> {
        return Observable.create(observer => {
            this.authHttp.get(`/api/experiments/${id}/result/${testNumber}`)
            .map((response: Response) => response.json())
            .subscribe(result => {
                var tests = new Array<Test>();
                
                console.log('Result of experiment');
                console.log(result);

                tests.push(new Test().deserialize(result));

                observer.next(tests);
                observer.complete();
            });
        });
    } 

    getByStatus(status: ExperimentStatus): Observable<Array<Experiment>> {
        return Observable.create(observer => {
            this.authHttp.get('/api/experiments/GetByStatus/' + status)
                .map((response: Response) => response.json())
                .subscribe((result) => {
                    var experiments = new Array<Experiment>();

                    //console.log('Result in experiment service: ');
                    //console.log(result);

                    for (let r of result) {
                        //console.log(r);
                        experiments.push(new Experiment().deserialize(r));
                    }
                    observer.next(experiments);
                    observer.complete();
                });
        });
    }
}