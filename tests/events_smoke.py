# Yevgeniy Mazur
# Event API checks.
import concurrent.futures
import datetime
import json
import os
import urllib.error
import urllib.request

BASE = os.environ.get('VIBEHIVE_API', 'http://localhost:5080')

def call(method, path, body=None, token=None, expected=200):
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    request = urllib.request.Request(BASE + path, data=None if body is None else json.dumps(body).encode(), headers=headers, method=method)
    try:
        response = urllib.request.urlopen(request)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        code, data = response.code, response.read()
    assert code == expected, (method, path, code, data)
    return json.loads(data) if data and code < 400 else None

call('POST', '/api/auth/register', {'name': 'Test Customer', 'email': 'customer@example.test', 'password': 'DemoPass123!'}, expected=201)
call('POST', '/api/auth/register', {'name': 'Duplicate', 'email': 'CUSTOMER@example.test', 'password': 'DemoPass123!'}, expected=409)
call('POST', '/api/auth/login', {'email': 'customer@example.test', 'password': 'wrong'}, expected=401)
customer = call('POST', '/api/auth/login', {'email': 'customer@example.test', 'password': 'DemoPass123!'})
assert customer['user']['role'] == 'Customer'
token = customer['token']
assert call('GET', '/api/users/me', token=token)['id'] == customer['user']['id']
call('GET', '/api/tickets', expected=401)
call('GET', '/api/tickets', token=token[:-8] + 'invalid!', expected=401)
organizer = call('POST', '/api/auth/login', {'email': os.environ['Organizer__Email'], 'password': os.environ['Organizer__Password']})['token']
event = {'title': 'VibeHive Concert', 'venue': 'Seattle', 'startsAt': (datetime.datetime.now(datetime.timezone.utc) + datetime.timedelta(days=2)).isoformat(), 'capacity': 2}
call('POST', '/api/events', event, expected=401)
call('POST', '/api/events', event, token, expected=403)
created = call('POST', '/api/events', event, organizer, expected=201)
call('POST', '/api/tickets', {'eventId': created['id'], 'quantity': 0}, token, expected=400)
booking = {'eventId': created['id'], 'quantity': 2}
# Check ticket capacity.
def competing(_):
    try:
        return call('POST', '/api/tickets', booking, token, expected=201)
    except AssertionError as error:
        assert error.args[0][2] == 409
        return None
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    results = list(pool.map(competing, range(2)))
assert sum(r is not None for r in results) == 1
assert call('GET', '/api/events')[0]['availableTickets'] == 0
ticket = next(r for r in results if r)
call('POST', '/api/auth/register', {'name': 'Other Customer', 'email': 'other@example.test', 'password': 'DemoPass123!'}, expected=201)
other = call('POST', '/api/auth/login', {'email': 'other@example.test', 'password': 'DemoPass123!'})['token']
call('DELETE', '/api/tickets/' + ticket['id'], token=other, expected=404)
call('DELETE', '/api/tickets/' + ticket['id'], token=token, expected=204)
call('DELETE', '/api/tickets/' + ticket['id'], token=token, expected=204)
assert call('GET', '/api/events')[0]['availableTickets'] == 2
assert call('GET', '/api/tickets', token=token)[0]['cancelled']
print('PASS: registration, login, JWT, roles, validation, concurrent capacity, ownership and cancellation')
