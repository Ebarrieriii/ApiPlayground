import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 100 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    'http_req_duration{api:FastEndpoints}': ['p(95)<1000'],
    'http_req_duration{api:Traditional}': ['p(95)<1000'],
  },
};

export default function () {
  const fastEndpointsRes = http.get('https://localhost:7153/posts?pageSize=5000', {
    tags: { api: 'FastEndpoints' },
  });
  check(fastEndpointsRes, {
    'FastEndpoints API - is status 200': (r) => r.status === 200,
  });

  const traditionalApiRes = http.get('https://localhost:7098/api/posts?pageSize=5000', {
    tags: { api: 'Traditional' },
  });
  check(traditionalApiRes, {
    'Traditional API - is status 200': (r) => r.status === 200,
  });

  sleep(1);
}
